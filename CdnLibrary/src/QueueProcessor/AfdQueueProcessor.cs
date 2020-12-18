/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    internal class AfdQueueProcessor : ICdnQueueProcessor<AfdRequest>
    {
        private readonly ILogger logger;
        private static readonly double RequestWaitTime = EnvironmentConfig.RequestWaitTime;

        public AfdQueueProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        public void AddMessageToQueue(CloudQueue outputQueue, CloudQueueMessage message, AfdRequest queueMsg)
        {
            // increase backoff based on # dequeues 
            var nextVisibleTime = queueMsg.NumTimesProcessed > 0 ? TimeSpan.FromSeconds(queueMsg.NumTimesProcessed * RequestWaitTime) : TimeSpan.FromSeconds(RequestWaitTime);

            outputQueue.AddMessage(message, null, nextVisibleTime);
        }

        public CloudQueueMessage CreateCloudQueueMessage(AfdRequest request, IRequestInfo requestInfo)
        {
            if (string.IsNullOrEmpty(request.Endpoint))
            {
                logger.LogWarning($"AfdQueueProcessor: Request with id={request.id} has empty endpoint");
                return null;
            }

            request.Status = requestInfo.RequestStatus.ToString();

            return new CloudQueueMessage(JsonSerializer.Serialize(request));
        }

        public async Task<CloudQueueMessage> ProcessPurgeRequest(AfdRequest queueMsg, int maxRetry)
        {
            if (queueMsg.Urls == null || queueMsg.Urls.Length == 0)
            {
                logger.LogWarning($"AfdQueueProcessor: Dropping purge msg with no urls");
                return null;
            }

            var requestInfo = new AfdRequestInfo() { RequestStatus = RequestStatus.MaxRetry };

            try
            {
                if (queueMsg.NumTimesProcessed < maxRetry)
                {
                    requestInfo = await AfdRequestProcessor.SendPurgeRequest(queueMsg.Endpoint, new StringContent(queueMsg.RequestBody, Encoding.UTF8, "application/json"), logger);

                    return CompletePurgeRequest(requestInfo, queueMsg);
                }
                else
                {
                    logger.LogWarning($"AfdQueueProcessor: Dropping purge msg: queueMsg with num NumTimesProcessed = {queueMsg.NumTimesProcessed} is greater than maxRetry={maxRetry}");
                }
            }
            finally
            {
                queueMsg.Status = requestInfo.RequestStatus.ToString();
            }

            return null;
        }

        public CloudQueueMessage CompletePurgeRequest(IRequestInfo requestInfo, AfdRequest queueMsg)
        {
            if (requestInfo.RequestStatus == RequestStatus.PurgeSubmitted && !string.IsNullOrEmpty(requestInfo.RequestID))
            {
                // If the request was submitted, add to queue as a poll request
                queueMsg.NumTimesProcessed = 0;
                queueMsg.CdnRequestId = requestInfo.RequestID;
            }
            else if (requestInfo.RequestStatus == RequestStatus.Throttled || requestInfo.RequestStatus == RequestStatus.Error)
            {
                requestInfo.RequestID = null;
                queueMsg.NumTimesProcessed++;
            }
            else if (requestInfo.RequestStatus == RequestStatus.Unauthorized || requestInfo.RequestStatus == RequestStatus.Unknown)
            {
                logger.LogError($"AfdQueueProcessor: Request Status {requestInfo.RequestStatus}, {queueMsg.id}");
                return null;
            }

            return CreateCloudQueueMessage(queueMsg, requestInfo);
        }

        public async Task<CloudQueueMessage> ProcessPollRequest(AfdRequest queueMsg, int maxRetry)
        {
            var requestInfo = new AfdRequestInfo() { RequestStatus = RequestStatus.MaxRetry };

            try
            {
                if (queueMsg.NumTimesProcessed < maxRetry)
                {
                    requestInfo.RequestStatus = await AfdRequestProcessor.SendPollRequest(queueMsg.Endpoint, queueMsg.CdnRequestId, logger);
                    return CompletePollRequest(requestInfo, queueMsg);
                }
                else
                {
                    logger.LogWarning($"AfdQueueProcessor: Dropping poll msg: queueMsg with num NumTimesProcessed = {queueMsg.NumTimesProcessed} is greater than maxRetry={maxRetry}");
                }
            }
            finally
            {
                queueMsg.Status = requestInfo.RequestStatus.ToString();
            }
            return null;
        }

        public CloudQueueMessage CompletePollRequest(IRequestInfo requestInfo, AfdRequest queueMsg)
        {
            if (requestInfo.RequestStatus == RequestStatus.PurgeCompleted)
            {
                logger.LogInformation($"AfdQueueProcessor: Completed purge request RequestID={queueMsg.CdnRequestId}, NumTimesProcessed={queueMsg.NumTimesProcessed}, {queueMsg.RequestBody}");
                return null;
            }
            else if (requestInfo.RequestStatus == RequestStatus.Throttled || requestInfo.RequestStatus == RequestStatus.Error)
            {
                // Only increase if the request is throttled or has an error
                // Otherwise, we can keep polling at the current rate until the purge is done
                queueMsg.NumTimesProcessed++;
            }

            //re-make polling message and re-add it to the queue
            return CreateCloudQueueMessage(queueMsg, requestInfo);
        }
    }
}
