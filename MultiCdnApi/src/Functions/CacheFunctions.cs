/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Swagger;

    public class CacheFunctions
    {
        private readonly IRequestTable<Partner> partnerTable;
        private readonly IRequestTable<UserRequest> userRequestTable;
        private readonly IPartnerRequestTableManager<CDN> partnerRequestTable;

        private readonly Metric urlsToPurgeSubmitted;
        private readonly Metric outgoingRequestsForCdnPlugins;

        public CacheFunctions(IRequestTable<Partner> partnerTable,
            IRequestTable<UserRequest> userRequestTable,
            IPartnerRequestTableManager<CDN> partnerRequestTable,
            TelemetryConfiguration telemetryConfiguration)
        {
            this.partnerTable = partnerTable;
            this.partnerRequestTable = partnerRequestTable;
            this.userRequestTable = userRequestTable;
            
            var telemetryClient = new TelemetryClient(telemetryConfiguration);
            urlsToPurgeSubmitted = telemetryClient.GetMetric($"{nameof(CreateCachePurgeRequestByHostname)} Urls To Purge");
            outgoingRequestsForCdnPlugins = telemetryClient.GetMetric($"{nameof(CreateCachePurgeRequestByHostname)} Outgoing Requests For CDN Plugins");
        }

        [PostContent("cachePurgeRequest", "Cache Purge Request: a JSON describing what urls to purge",
            @"{" + "\n"
                 + @"    ""Description"": ""Operation Description""," + "\n"
                 + @"    ""Hostname"": ""Purge Hostname""," + "\n"
                 + @"    ""Urls"": [" + "\n"
                 + @"        ""url1""," + "\n"
                 + @"        ""url2""" + "\n"
                 + @"    ]" + "\n"
                 + @"}")]
        [FunctionName("CreateCachePurgeRequestByHostname")]
        public async Task<IActionResult> CreateCachePurgeRequestByHostname(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{partnerId:guid}/CachePurgeByHostname")]
            HttpRequest req,
            string partnerId,
            ILogger log)
        {
            UserGroupAuthValidator.CheckUserAuthorized(req);
            
            log.LogInformation($"{nameof(CreateCachePurgeRequestByHostname)}; " +
                               $"invoked by {req.HttpContext.User?.Identity?.Name}");
            try
            {
                if (partnerId == null)
                {
                    return new StringResult("Invalid Partner Id");
                }

                var bodyContent = await new StreamReader(req.Body).ReadToEndAsync();
                var purgeRequest = JsonSerializer.Deserialize<PurgeRequest>(bodyContent,
                    new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Skip});
                var description = purgeRequest.Description;
                var ticketId = purgeRequest.TicketId;
                var hostname = purgeRequest.Hostname;
                var urls = ResolveUrls(hostname, purgeRequest.Urls);
                log.LogInformation($"{nameof(CreateCachePurgeRequestByHostname)}: purging {urls.Count} urls for partner {partnerId}");

                var partner = await partnerTable.GetItem(partnerId);
                var userRequest = new UserRequest(partner.id, description, ticketId, hostname, urls);

                await userRequestTable.CreateItem(userRequest);
                var userRequestId = userRequest.id;

                foreach (var partnerCdnConfiguration in partner.CdnConfigurations)
                {
                    var cdnWithCredentials = partnerCdnConfiguration.CdnWithCredentials;
                    foreach (var cdnWithCredential in cdnWithCredentials)
                    {
                        var cdn = Enum.Parse<CDN>(cdnWithCredential.Key);
                        var partnerRequest = CdnRequestHelper.CreatePartnerRequest(cdn, partner, userRequest, description, ticketId);
                        await partnerRequestTable.CreatePartnerRequest(partnerRequest, cdn);
                        userRequest.NumTotalPartnerRequests++;
                    }
                }

                await userRequestTable.UpsertItem(userRequest);
                urlsToPurgeSubmitted.TrackValue(urls.Count);
                outgoingRequestsForCdnPlugins.TrackValue(userRequest.NumTotalPartnerRequests);
                return new StringResult(userRequestId); 
            }
            catch (Exception e)
            {
                return new ExceptionResult(e);
            }
        }

        [FunctionName("CachePurgeRequestByHostnameStatus")]
        public async Task<IActionResult> CachePurgeRequestByHostnameStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{partnerId}/CachePurgeStatus/{userRequestId}")]
            HttpRequest req,
            string partnerId,
            string userRequestId,
            ILogger log)
        {
            UserGroupAuthValidator.CheckUserAuthorized(req);

            log.LogInformation($"{nameof(CachePurgeRequestByHostnameStatus)}: {userRequestId} (partnerId={partnerId});" +
                               $"invoked by {req.HttpContext.User?.Identity?.Name}");
            try
            {
                var userRequest = await userRequestTable.GetItem(userRequestId);
                return new UserRequestStatusResult(userRequest);
            }
            catch (Exception e)
            {
                log.LogInformation($"{nameof(CachePurgeRequestByHostnameStatus)}: got exception {e.Message}; {e.StackTrace}");
                return new ExceptionResult(e);
            }
        }
        
        
        private ISet<string> ResolveUrls(string hostname, IEnumerable<string> purgeRequestUrls)
        {
            var result = new HashSet<string>();
            Uri parsedHostname = null;
            if (!string.IsNullOrWhiteSpace(hostname))
            {
                parsedHostname = new Uri(hostname);
            }
            foreach (var purgeRequestUrl in purgeRequestUrls)
            {
                var parsedUrl = new Uri(purgeRequestUrl);
                if (parsedUrl.IsAbsoluteUri)
                {
                    result.Add(purgeRequestUrl);
                }
                else
                {
                    if (parsedHostname == null)
                    {
                        throw new InvalidOperationException("Urls are not absolute, but the hostname is empty");
                    }
                    parsedUrl = new Uri(parsedHostname, purgeRequestUrl);
                    result.Add(parsedUrl.ToString());
                }
            }
            return result;
        }
    }
}