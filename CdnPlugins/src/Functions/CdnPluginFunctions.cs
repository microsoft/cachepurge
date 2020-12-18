/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

/* 
 * Documentation Links:
 *   CosmosDB Trigger: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-trigger?tabs=csharp
 *   Azure Queue Output: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output?tabs=csharp
 */

namespace CdnPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    public class CdnPluginFunctions
    {
        private readonly IPartnerRequestTableManager<CDN> partnerRequestTablesManager;

        public CdnPluginFunctions(IPartnerRequestTableManager<CDN> partnerRequestTablesManager) 
        { 
            this.partnerRequestTablesManager = partnerRequestTablesManager;
        }

        [FunctionName("AkamaiPlugin")]
        public async Task PurgeAkamai(
            [CosmosDBTrigger(
                databaseName: EnvironmentConfig.DatabaseName,
                collectionName: EnvironmentConfig.AkamaiPartnerCollectionName,
                ConnectionStringSetting = EnvironmentConfig.CosmosDBConnectionStringName,
                LeaseCollectionName = "partnerCollectionLeases",
                CreateLeaseCollectionIfNotExists = true)]
                IReadOnlyList<Document> partnerRequests,
            [Queue(
                EnvironmentConfig.AkamaiBatchQueueName),
                StorageAccount(EnvironmentConfig.BatchQueueConnectionStringName)]
                ICollector<ICdnRequest> queue, 
            ILogger log)
        {
            log.LogInformation($"{nameof(PurgeAkamai)} ({nameof(CdnPluginFunctions)}) query: {partnerRequests.Count}");

            if (partnerRequests == null || partnerRequests.Count <= 0) { throw new ArgumentNullException(nameof(partnerRequests)); }

            var plugin = new AkamaiPlugin(log);

            foreach (var r in partnerRequests)
            {
                if (plugin.ValidPartnerRequest(r.ToString(), r.Id, out var partnerRequest) && plugin.ProcessPartnerRequest(partnerRequest, queue))
                {
                    // If cdnRequest creation was successful, update the PartnerRequest in DB with info 
                    // regarding # of batch requests created
                    await partnerRequestTablesManager.UpdatePartnerRequest(partnerRequest, CDN.Akamai);
                }
            }
            log.LogInformation($"{nameof(PurgeAkamai)} ({nameof(CdnPluginFunctions)}) finished processing");
        }

        [FunctionName("AfdPlugin")]
        public async Task PurgeAfd(
            [CosmosDBTrigger(
                databaseName: EnvironmentConfig.DatabaseName,
                collectionName: EnvironmentConfig.AfdPartnerCollectionName,
                ConnectionStringSetting = EnvironmentConfig.CosmosDBConnectionStringName,
                LeaseCollectionName = "partnerCollectionLeases",
                CreateLeaseCollectionIfNotExists = true)]
                IReadOnlyList<Document> partnerRequests,
            [Queue(
                EnvironmentConfig.AfdBatchQueueName),
                StorageAccount(EnvironmentConfig.BatchQueueConnectionStringName)]
                ICollector<ICdnRequest> queue,
            ILogger log)
        {
            log.LogInformation($"{nameof(PurgeAfd)} ({nameof(CdnPluginFunctions)}) query: {partnerRequests.Count}");

            if (partnerRequests == null || partnerRequests.Count <= 0) { throw new ArgumentNullException(nameof(partnerRequests)); }

            var plugin = new AfdPlugin(log);

            foreach (var r in partnerRequests)
            {
                if (plugin.ValidPartnerRequest(r.ToString(), r.Id, out var partnerRequest) && plugin.ProcessPartnerRequest(partnerRequest, queue))
                {
                    // If cdnRequest creation was successful, update the PartnerRequest in DB with info 
                    // regarding # of batch requests created
                    await partnerRequestTablesManager.UpdatePartnerRequest(partnerRequest, CDN.AFD);
                }
            }
            log.LogInformation($"{nameof(PurgeAfd)} ({nameof(CdnPluginFunctions)}) finished processing");
        }
    }
}
