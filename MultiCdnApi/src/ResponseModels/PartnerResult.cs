/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using CachePurgeLibrary;
    using Microsoft.AspNetCore.Mvc;

    public class PartnerResult : JsonResult
    {
        public PartnerResult(Partner partner) : base(new object())
        {
            var cdnConfigurationResults = new List<CdnConfigurationValue>();
            var partnerCdnConfigurations = partner.CdnConfigurations;
            foreach (var cdnConfiguration in partnerCdnConfigurations)
            {
                var credentials = new Dictionary<string, string>();
                foreach (var cdnConfigurationCredentialKey in cdnConfiguration.CdnWithCredentials.Keys)
                {
                    credentials[cdnConfigurationCredentialKey] = string.Empty;
                }
                
                cdnConfigurationResults.Add(new CdnConfigurationValue
                {
                    Hostname = cdnConfiguration.Hostname,
                    CdnCredentials = credentials 
                });
            }
            Value = new PartnerValue
            {
                Id = partner.id,
                TenantId = partner.TenantId,
                Name = partner.Name,
                ContactEmail = partner.ContactEmail,
                NotifyContactEmail = partner.NotifyContactEmail,
                CdnConfigurations = cdnConfigurationResults
            };
        }
    }

    public class PartnerValue
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string NotifyContactEmail { get; set; }
        public List<CdnConfigurationValue> CdnConfigurations { get; set; }
    }

    public class CdnConfigurationValue
    {
        public string Hostname { get; set; }
        public IDictionary<string, string> CdnCredentials { get; set; }
    }  
}