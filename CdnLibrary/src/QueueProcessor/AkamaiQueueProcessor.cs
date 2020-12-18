/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CachePurgeLibrary;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Logging;

    internal class AkamaiQueueProcessor : ICdnQueueProcessor<AkamaiRequest>
    {
        private readonly ILogger logger;
        private static readonly double RequestWaitTime = EnvironmentConfig.RequestWaitTime;

        public AkamaiQueueProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        public CloudQueueMessage CreateCloudQueueMessage(AkamaiRequest request, IRequestInfo requestInfo)
        {
            if (string.IsNullOrEmpty(request.Endpoint))
            {
                logger.LogWarning($"AkamaiQueueProcessor: Request with id={request.id} has empty endpoint");
                return null;
            }

            request.Status = requestInfo.RequestStatus.ToString();

            return new CloudQueueMessage(JsonSerializer.Serialize(request));
        }

        public void AddMessageToQueue(CloudQueue outputQueue, CloudQueueMessage message, AkamaiRequest queueMsg)
        {
            // increase backoff based on # dequeues 
            var nextVisibleTime = queueMsg.NumTimesProcessed > 0 ? TimeSpan.FromSeconds(queueMsg.NumTimesProcessed * RequestWaitTime) : TimeSpan.FromSeconds(RequestWaitTime);

            outputQueue.AddMessage(message, null, nextVisibleTime);
        }

        public async Task<CloudQueueMessage> ProcessPurgeRequest(AkamaiRequest queueMsg, int maxRetry)
        {
            if (queueMsg.Urls == null || queueMsg.Urls.Length == 0)
            {
                logger.LogWarning($"AkamaiQueueProcessor: Dropping purge msg with no urls");
                return null;
            }

            var requestInfo = new AkamaiRequestInfo() { RequestStatus = RequestStatus.MaxRetry };

            try
            {
                if (queueMsg.NumTimesProcessed < maxRetry)
                {
                    requestInfo = await AkamaiRequestProcessor.SendPurgeRequest(queueMsg.Endpoint, new StringContent(queueMsg.RequestBody, Encoding.UTF8, "application/json"), logger);

                    return CompletePurgeRequest(requestInfo, queueMsg);
                }
                else
                {
                    logger.LogWarning($"AkamaiQueueProcessor: Dropping purge msg: queueMsg with num NumTimesProcessed = {queueMsg.NumTimesProcessed} is greater than maxRetry={maxRetry}");
                }
            }
            finally
            {
                queueMsg.Status = requestInfo.RequestStatus.ToString();
            }

            return null;
        }

        public CloudQueueMessage CompletePurgeRequest(IRequestInfo requestInfo, AkamaiRequest queueMsg)
        {
            if (requestInfo.RequestStatus == RequestStatus.PurgeCompleted && !string.IsNullOrEmpty(requestInfo.RequestID))
            {
                // Don't need to poll Akamai to verify purge completion
                queueMsg.CdnRequestId = requestInfo.RequestID;

                if (requestInfo is AkamaiRequestInfo akamaiRequestInfo)
                {
                    queueMsg.SupportId = akamaiRequestInfo.SupportID;
                }

                return null;
            }
            else if (requestInfo.RequestStatus == RequestStatus.Throttled || requestInfo.RequestStatus == RequestStatus.Error)
            {
                requestInfo.RequestID = null;
                queueMsg.NumTimesProcessed++;
            }
            else if (requestInfo.RequestStatus == RequestStatus.Unauthorized || requestInfo.RequestStatus == RequestStatus.Unknown)
            {
                logger.LogError($"AkamaiQueueProcessor: Request Status {requestInfo.RequestStatus}, {queueMsg.id}");
                return null;
            }

            return CreateCloudQueueMessage(queueMsg, requestInfo);
        }

        public CloudQueueMessage CompletePollRequest(IRequestInfo requestInfo, AkamaiRequest queueMsg) => throw new NotImplementedException();

        public Task<CloudQueueMessage> ProcessPollRequest(AkamaiRequest queueMsg, int maxRetry) => throw new NotImplementedException();
    }
}
