/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

/* 
 * Documentation Links:
 *   Azure Queue Trigger: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=csharp
 *   Azure Queue Output: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output?tabs=csharp
 */

namespace CdnPlugin
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using System.Collections;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class QueueProcessorFunctions 
    { 
        private static readonly int MaxRetry = EnvironmentConfig.MaxRetry;
        private readonly ICdnRequestTableManager<CDN> cdnRequestTableManager;

        public QueueProcessorFunctions(ICdnRequestTableManager<CDN> cdnRequestTableManager)
        {
            this.cdnRequestTableManager = cdnRequestTableManager;
        }

        [FunctionName("AfdQueueProcessor")]
        public async Task ProcessAfd(
            [QueueTrigger(EnvironmentConfig.AfdBatchQueueName, Connection = EnvironmentConfig.BatchQueueConnectionStringName)] CloudQueueMessage queueMessage,
            [Queue(EnvironmentConfig.AfdBatchQueueName), StorageAccount(EnvironmentConfig.BatchQueueConnectionStringName)] CloudQueue outputQueue,
            ILogger logger)
        {
            logger.LogInformation($"{nameof(ProcessAfd)} ({nameof(QueueProcessorFunctions)}) start: {queueMessage}");

            var message = queueMessage.AsString;

            if (string.IsNullOrEmpty(message))
            {
                logger.LogWarning("AfdQueueProcessor: QueueMessage is empty string");
                return;
            }

            var queueMsg = JsonSerializer.Deserialize<AfdRequest>(message);

            if (!ValidCdnRequest(queueMsg))
            {
                logger.LogError("AfdQueueProcessor: Invalid CdnRequest");
                return;
            }

            var queueProcessor = new AfdQueueProcessor(logger);

            CloudQueueMessage msg = !string.IsNullOrEmpty(queueMsg.CdnRequestId) ? await queueProcessor.ProcessPollRequest(queueMsg, MaxRetry) : await queueProcessor.ProcessPurgeRequest(queueMsg, MaxRetry);

            if (CdnQueueHelper.AddCdnRequestToDB(queueMsg, MaxRetry))
            {
                await cdnRequestTableManager.UpdateCdnRequest(queueMsg, CDN.AFD);
            }

            if (msg != null) { queueProcessor.AddMessageToQueue(outputQueue, msg, queueMsg); }
            
            logger.LogInformation($"{nameof(ProcessAfd)} ({nameof(QueueProcessorFunctions)}) finished processing");
        }

        [FunctionName("AkamaiQueueProcessor")]
        public async Task ProcessAkamai(
            [QueueTrigger(EnvironmentConfig.AkamaiBatchQueueName, Connection = EnvironmentConfig.BatchQueueConnectionStringName)] CloudQueueMessage queueMessage,
            [Queue(EnvironmentConfig.AkamaiBatchQueueName), StorageAccount(EnvironmentConfig.BatchQueueConnectionStringName)] CloudQueue outputQueue,
            ILogger logger)
        {
            logger.LogInformation($"{nameof(ProcessAkamai)} ({nameof(QueueProcessorFunctions)}) start: {queueMessage}");

            var message = queueMessage.AsString;

            if (string.IsNullOrEmpty(message))
            {
                logger.LogWarning("AkamaiQueueProcessor: QueueMessage is empty string");
                return;
            }

            var queueMsg = JsonSerializer.Deserialize<AkamaiRequest>(message);

            if (!ValidCdnRequest(queueMsg))
            {
                logger.LogError("AkamaiQueueProcessor: Invalid CdnRequest");
                return;
            }

            if (string.IsNullOrEmpty(queueMsg.CdnRequestId))
            {
                var queueProcessor = new AkamaiQueueProcessor(logger);

                var msg = await queueProcessor.ProcessPurgeRequest(queueMsg, MaxRetry);

                if (CdnQueueHelper.AddCdnRequestToDB(queueMsg, MaxRetry))
                {
                    await cdnRequestTableManager.UpdateCdnRequest(queueMsg, CDN.Akamai);
                }

                if (msg != null) { queueProcessor.AddMessageToQueue(outputQueue, msg, queueMsg); }
            }
            logger.LogInformation($"{nameof(ProcessAkamai)} ({nameof(AkamaiQueueProcessor)}) finished processing");
        }

        private bool ValidCdnRequest(ICdnRequest cdnRequest)
        {
            if (cdnRequest != null && !string.IsNullOrEmpty(cdnRequest.id) && cdnRequest.Urls != null && cdnRequest.Urls.Length > 0 && cdnRequest.RequestBody != null)
            {
                return true;
            }
            return false;
        }
    }
}
