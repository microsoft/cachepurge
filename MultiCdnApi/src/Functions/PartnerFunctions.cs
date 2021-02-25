/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CachePurgeLibrary;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Swagger;

    public class PartnerFunctions
    {
        private const string PartnerIdParameter = "partnerId";

        private readonly IRequestTable<Partner> partnerTable;

        public PartnerFunctions(IRequestTable<Partner> partnerTable)
        {
            this.partnerTable = partnerTable;
        }
        
        [FunctionName("GetPartner")]
        public async Task<IActionResult> GetPartner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "partners/{partnerId:guid}")]
            HttpRequest req,
            Guid partnerId,
            ILogger log)
        {
            try
            {
                var partner = await partnerTable.GetItem(partnerId.ToString());
                return new PartnerResult(partner);
            }
            catch (Exception e)
            {
                return new ExceptionResult(e);
            }
        }
        
        [PostContent("CreatePartner", "Create new Partner",
            @"{" + "\n"
            + @"  ""Tenant"": ""tenant_name"", // for example, 'Bing'. Note: the tenant name is used in AFD API to purge caches" + "\n"
            + @"  ""Name"": ""partner_name"", // for example, 'Bing_Multimedia'. Note: the name is used in AFD API to purge caches" + "\n"
            + @"  ""ContactEmail"": ""contact@example.com"", // contact email" + "\n"
            + @"  ""NotifyContactEmail"": ""notify@example.com"", // email for notifications" + "\n"
            + @"  ""CdnConfiguration"": {" + "\n"
            + @"     ""Hostname"": """"," + "\n"
            + @"     ""CdnWithCredentials"": {" + "\n"
            + @"        ""AFD"":"""", // Can be empty (default auth will be used)" + "\n"
            + @"        ""Akamai"": """" // Can be empty (default auth will be used)" + "\n"
            + @"     }" 
            + @"  }" + "\n"
            + @"}")]
        [FunctionName("CreatePartner")]
        public async Task<IActionResult> CreatePartner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "partners")]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                var requestContent = await new StreamReader(req.Body).ReadToEndAsync();
                var createPartnerRequest = JsonSerializer.Deserialize<PartnerConfigRequest>(requestContent, new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Skip});
                
                log.LogInformation($"{nameof(CreatePartner)}: {createPartnerRequest}");

                var tenant = createPartnerRequest.Tenant;
                var name = createPartnerRequest.Name;
                var contactEmail = createPartnerRequest.ContactEmail;
                var notifyContactEmail = createPartnerRequest.NotifyContactEmail;
                var cdnConfiguration = createPartnerRequest.CdnConfiguration;
                var partner = new Partner(tenant, name, contactEmail, notifyContactEmail, new[] { cdnConfiguration });
                await partnerTable.CreateItem(partner);
                return new StringResult(partner.id);
            }
            catch (Exception e)
            {
                return new ExceptionResult(e);
            }
        }

        [FunctionName("ListPartners")]
        public async Task<IActionResult> ListPartners(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "partners")]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                var partners = await partnerTable.GetItems();
                return new EnumerableResult<PartnerResult>(partners.Select(p => new PartnerResult(p)).ToList());
            }
            catch (Exception e)
            {
                return new ExceptionResult(e);
            }
        }
    }
}