/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;
    using CdnLibrary;
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

    public class CacheFunctions
    {
        // todo: handle exceptions and logging across all Azure Functions
        private readonly IRequestTable<Partner> partnerTable;
        private readonly IRequestTable<UserRequest> userRequestTable;
        private readonly IPartnerRequestTableManager<CDN> partnerRequestTable;

        public CacheFunctions(IRequestTable<Partner> partnerTable,
            IRequestTable<UserRequest> userRequestTable,
            IPartnerRequestTableManager<CDN> partnerRequestTable)
        {
            this.partnerTable = partnerTable;
            this.partnerRequestTable = partnerRequestTable;
            this.userRequestTable = userRequestTable;
        }

        [FunctionName("CreateCachePurgeRequestByHostname")]
        public async Task<IActionResult> CreateCachePurgeRequestByHostname(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{partnerId:guid}/CachePurgeByHostname")]
            HttpRequest req,
            string partnerId,
            ILogger log)
        {
            log.LogInformation($"{nameof(CreateCachePurgeRequestByHostname)}");

            try
            {
                if (partnerId == null)
                {
                    return new StringResult("Invalid Partner Id");
                }

                var bodyContent = await new StreamReader(req.Body).ReadToEndAsync();
                var purgeRequest = JsonSerializer.Deserialize<PurgeRequest>(bodyContent);
                var urls = new HashSet<string>(purgeRequest.Urls);
                var description = purgeRequest.Description;
                var ticketId = purgeRequest.TicketId;
                var hostname = purgeRequest.Hostname;
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
            log.LogInformation($"{nameof(CachePurgeRequestByHostnameStatus)}: {userRequestId} (partnerId={partnerId})");
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
    }
}