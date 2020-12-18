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

    internal class AkamaiPlugin : ICdnPlugin<AkamaiPartnerRequest>
    {
        private const string Production = "production";
        private const string Staging = "staging";

        private static readonly string baseUri = Environment.GetEnvironmentVariable("Akamai_BaseUri") ?? "https://fakeUri";
        private static readonly string BatchCreated = RequestStatus.BatchCreated.ToString();

        private readonly int maxNumUrl = EnvironmentConfig.AkamaiBatchSize;

        private readonly ILogger logger;

        // TODO: Implement for delete
        public AkamaiPlugin(ILogger logger)
        {
            this.logger = logger;
        }

        public AkamaiPlugin(ILogger logger, int maxNumUrl)
        {
            this.logger = logger;
            this.maxNumUrl = maxNumUrl;
        }

        public bool ProcessPartnerRequest(AkamaiPartnerRequest partnerRequest, ICollector<ICdnRequest> queue)
        {
            var endpoint = GetEndpoint(partnerRequest);

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

        public void AddMessagesToSendQueue(ICdnRequest cdnRequest, ICollector<ICdnRequest> msg)
        {
            if (cdnRequest.Urls.Length > 0)
            {
                try
                {
                    var akamaiRequestBody = new AkamaiRequestBody()
                    {
                        Objects = cdnRequest.Urls,
                    };

                    cdnRequest.RequestBody = JsonSerializer.Serialize(akamaiRequestBody, CdnPluginHelper.JsonSerializerOptions);

                    msg.Add(cdnRequest);
                }
                catch (Exception e)
                {
                    logger.LogError($"AkamaiPlugin: Exception serializing cdnRequest id={cdnRequest.id}, {e.Message}");
                }
            }
        }

        public IList<ICdnRequest> SplitRequestIntoBatches(AkamaiPartnerRequest partnerRequest, int maxNumUrl)
        {
            var partnerRequests = new List<ICdnRequest>();

            var urlLists = CdnPluginHelper.SplitUrlListIntoBatches(partnerRequest.Urls, maxNumUrl);
            partnerRequest.NumTotalCdnRequests = urlLists.Count;

            foreach (var list in urlLists)
            {
                var req = new AkamaiRequest(partnerRequest.id, list);
                partnerRequests.Add(req);
            }

            return partnerRequests;
        }

        public string GetEndpoint(AkamaiPartnerRequest partnerRequest)
        {
            // Use staging as default
            var endpoint = $"{baseUri}{Staging}";

            if (!string.IsNullOrEmpty(partnerRequest.Network) && partnerRequest.Network.Equals(Production, StringComparison.OrdinalIgnoreCase))
            {
                endpoint = $"{baseUri}{Production}";
            }

            return endpoint;
        }

        public bool ValidPartnerRequest(string inputRequest, string resourceID, out AkamaiPartnerRequest partnerRequest)
        {
            partnerRequest = null;

            try
            {
                partnerRequest = JsonSerializer.Deserialize<AkamaiPartnerRequest>(inputRequest, CdnPluginHelper.JsonSerializerOptions);

                return CdnPluginHelper.IsValidRequest(partnerRequest);
            }
            catch (Exception e)
            {
                logger.LogError($"AkamaiPlugin: Exception reading resource ID={resourceID}, {e.Message}");
            }

            return false;
        }
    }
}

