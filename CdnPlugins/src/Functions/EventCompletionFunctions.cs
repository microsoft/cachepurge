/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

/* 
 * Documentation Links:
 *   CosmosDB Trigger: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-trigger?tabs=csharp
 */

namespace CdnPlugin
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class EventCompletionFunctions
    {
        private static readonly string PurgeCompleted = RequestStatus.PurgeCompleted.ToString();
        private static readonly string BatchCreated = RequestStatus.BatchCreated.ToString();
        private static readonly string PurgeSubmitted = RequestStatus.PurgeSubmitted.ToString();

        private readonly IRequestTable<UserRequest> userRequestTable;
        private readonly IPartnerRequestTableManager<CDN> partnerRequestTableManager;

        public EventCompletionFunctions(IRequestTable<UserRequest> userRequestTable, IPartnerRequestTableManager<CDN> partnerRequestTableManager)
        {
            this.userRequestTable = userRequestTable;
            this.partnerRequestTableManager = partnerRequestTableManager;
        }

        internal async Task CompletePurge(ICdnRequest cdnRequest, CDN cdn, ILogger logger)
        {
            try
            {
                if (cdnRequest.Status.Equals(PurgeSubmitted))
                {
                    return;
                }

                var partnerReq = await partnerRequestTableManager.GetPartnerRequest(cdnRequest.PartnerRequestID, cdn);
                
                if (partnerReq != null && UpdatePartnerRequest(partnerReq, cdnRequest.Status, logger))
                {
                    await partnerRequestTableManager.UpdatePartnerRequest(partnerReq, cdn);

                    await UpdateUserRequest(partnerReq, logger);
                }
            }
            catch(Exception e)
            {
                var exceptionMsg = e.InnerException?.Message ?? e.Message;
                logger.LogError(exceptionMsg);
            }
        }

        [FunctionName("AfdEventCompletion")]
        public async Task CompleteAfd(
            [CosmosDBTrigger(
                databaseName: EnvironmentConfig.DatabaseName,
                collectionName: EnvironmentConfig.AfdCdnCollectionName,
                ConnectionStringSetting = EnvironmentConfig.CosmosDBConnectionStringName,
                LeaseCollectionName = "cdnCollectionLeases",
                CreateLeaseCollectionIfNotExists = true)]
                IReadOnlyList<Document> afdRequests,
            ILogger logger)
        {
            logger.LogInformation($"{nameof(CompleteAfd)} ({nameof(EventCompletionFunctions)}) starting: {afdRequests.Count} requests");

            if (afdRequests == null || afdRequests.Count == 0) { return; }

            foreach (var r in afdRequests)
            {
                var purgeRequest = JsonSerializer.Deserialize<AfdRequest>(r.ToString(), CdnPluginHelper.JsonSerializerOptions);

                await CompletePurge(purgeRequest, CDN.AFD, logger);
            } 
            
            logger.LogInformation($"{nameof(CompleteAfd)} ({nameof(EventCompletionFunctions)}) finished processing");
        }

        [FunctionName("AkamaiEventCompletion")]
        public async Task CompleteAkamai(
           [CosmosDBTrigger(
                databaseName: EnvironmentConfig.DatabaseName,
                collectionName: EnvironmentConfig.AkamaiCdnCollectionName,
                ConnectionStringSetting = EnvironmentConfig.CosmosDBConnectionStringName,
                LeaseCollectionName = "cdnCollectionLeases",
                CreateLeaseCollectionIfNotExists = true)]
                IReadOnlyList<Document> akamaiRequests,
           ILogger logger)
        {
            logger.LogInformation($"{nameof(CompleteAkamai)} ({nameof(EventCompletionFunctions)}) starting: {akamaiRequests.Count} requests");

            if (akamaiRequests == null || akamaiRequests.Count == 0) { return; }

            foreach (var r in akamaiRequests)
            {
                var purgeRequest = JsonSerializer.Deserialize<AkamaiRequest>(r.ToString(), CdnPluginHelper.JsonSerializerOptions);

                await CompletePurge(purgeRequest, CDN.Akamai, logger);
            }
            
            logger.LogInformation($"{nameof(CompleteAkamai)} ({nameof(EventCompletionFunctions)}) finished processing");
        }

        internal bool UpdatePartnerRequest(IPartnerRequest partnerRequest, string cdnRequestStatus, ILogger logger)
        {
            try
            {
                partnerRequest.NumCompletedCdnRequests++;

                // If error status set the parentRequest status to the same value
                if (!cdnRequestStatus.Equals(PurgeCompleted))
                {
                    partnerRequest.Status = cdnRequestStatus;
                }

                // ParentRequest status is only overwritten if it's got the default value of created
                // to prevent overwriting an error status 
                if (partnerRequest.NumCompletedCdnRequests >= partnerRequest.NumTotalCdnRequests && partnerRequest.Status.Equals(BatchCreated))
                {
                    partnerRequest.Status = cdnRequestStatus;
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return false;
            }
        }

        internal async Task UpdateUserRequest(IPartnerRequest partnerRequest, ILogger logger)
        {
            try
            {
                // if the partner request is complete, update the user request
                if (partnerRequest.NumCompletedCdnRequests >= partnerRequest.NumTotalCdnRequests)
                {
                    var userReq = await userRequestTable.GetItem(partnerRequest.UserRequestID);

                    if (userReq != null && userReq.NumCompletedPartnerRequests <= userReq.NumTotalPartnerRequests)
                    {
                        if (RequestStatus.PurgeCompleted.ToString().Equals(partnerRequest.Status))
                        {
                            userReq.NumCompletedPartnerRequests++;
                        }
                        userReq.PluginStatuses[partnerRequest.CDN] = partnerRequest.Status;
                        await userRequestTable.UpsertItem(userReq);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}
