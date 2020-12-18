/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    internal class AfdPlugin : ICdnPlugin<AfdPartnerRequest>
    {
        private static readonly string baseUri = Environment.GetEnvironmentVariable("Afd_BaseUri") ?? "https://www.afdcp.com/api/v2.0/Tenants/";
        private static readonly string BatchCreated = RequestStatus.BatchCreated.ToString();
        private readonly int maxNumUrl = EnvironmentConfig.AfdBatchSize;

        private readonly ILogger logger;

        public AfdPlugin(ILogger logger)
        {
            this.logger = logger;
        }

        public AfdPlugin(ILogger logger, int maxNumUrl)
        {
            this.logger = logger;
            this.maxNumUrl = maxNumUrl;
        }

        public bool ProcessPartnerRequest(AfdPartnerRequest partnerRequest, ICollector<ICdnRequest> queue)
        {
            if (TryGetEndpoint(partnerRequest, out var endpoint))
            {
                // Split messages into batches and add to queue
                var batchedRequests = SplitRequestIntoBatches(partnerRequest, maxNumUrl);

                foreach (var req in batchedRequests)
                {
                    req.Endpoint = endpoint;
                    AddMessagesToSendQueue(req, queue);
                }

                partnerRequest.Status = BatchCreated;

                return true;
            }

            return false;
        }

        public void AddMessagesToSendQueue(ICdnRequest cdnRequest, ICollector<ICdnRequest> msg)
        {
            string desc = $"CachePurge_{DateTime.UtcNow}";
            if (cdnRequest is AfdRequest afdRequest && !string.IsNullOrEmpty(afdRequest.Description))
            {
                desc = afdRequest.Description;
            }

            if (cdnRequest.Urls.Length > 0)
            {
                try
                {
                    var afdRequestBody = new AfdRequestBody()
                    {
                        Urls = cdnRequest.Urls,
                        Description = desc
                    };

                    cdnRequest.RequestBody = JsonSerializer.Serialize(afdRequestBody, CdnPluginHelper.JsonSerializerOptions);

                    msg.Add(cdnRequest);
                }
                catch (Exception e)
                {
                    logger.LogError($"AfdPlugin: Exception serializing cdnRequest id={cdnRequest.id}, {e.Message}");
                }
            }
        }

        public IList<ICdnRequest> SplitRequestIntoBatches(AfdPartnerRequest partnerRequest, int maxNumUrl)
        {
            var partnerRequests = new List<ICdnRequest>();

            var urlLists = CdnPluginHelper.SplitUrlListIntoBatches(partnerRequest.Urls, maxNumUrl);
            partnerRequest.NumTotalCdnRequests = urlLists.Count;

            foreach (var list in urlLists)
            {
                var req = new AfdRequest(partnerRequest.id, partnerRequest.TenantID, partnerRequest.PartnerID, partnerRequest.Description, list);
                partnerRequests.Add(req);
            }

            return partnerRequests;
        }

        public bool TryGetEndpoint(AfdPartnerRequest partnerRequest, out string endpoint)
        {
            endpoint = string.Empty;

            if (string.IsNullOrEmpty(partnerRequest.TenantID) || string.IsNullOrEmpty(partnerRequest.PartnerID))
            {
                return false;
            }

            endpoint = baseUri + $"{partnerRequest.TenantID}/Partners/{partnerRequest.PartnerID}/CachePurges";

            return true;
        }

        public bool ValidPartnerRequest(string inputRequest, string resourceID, out AfdPartnerRequest partnerRequest)
        {
            partnerRequest = null;

            try
            {
                partnerRequest = JsonSerializer.Deserialize<AfdPartnerRequest>(inputRequest, CdnPluginHelper.JsonSerializerOptions);

                return CdnPluginHelper.IsValidRequest(partnerRequest);
            }
            catch (Exception e)
            {
                logger.LogError($"AfdPlugin: Exception reading resource ID={resourceID}, {e.Message}");
            }

            return false;
        }
    }
}
