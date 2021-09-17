/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.AspNetCore.Mvc;

    public class PartnerResult : JsonResult
    {
        public PartnerResult(Partner partner) : base(new object())
        {
            var cdnConfiguration = partner.CdnConfiguration;
            IDictionary<string, bool> pluginIsEnabled;
            if (cdnConfiguration == null || cdnConfiguration.PluginIsEnabled == null)
            { // handling old data while it's not updated yet
                pluginIsEnabled = new Dictionary<string, bool> {
                    { CDN.AFD.ToString() , true},
                    { CDN.Akamai.ToString() , true},
                };
            }
            else
            {
                pluginIsEnabled = cdnConfiguration.PluginIsEnabled;
            }

            var cdnConfigurationValue = new CdnConfigurationValue {
                Hostname = cdnConfiguration?.Hostname ?? "",
                PluginIsEnabled = pluginIsEnabled
            };
            Value = new PartnerValue
            {
                Id = partner.id,
                TenantId = partner.TenantId,
                Name = partner.Name,
                CdnConfiguration = cdnConfigurationValue
            };
        }
    }

    public class PartnerValue
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public CdnConfigurationValue CdnConfiguration { get; set; }
    }

    public class CdnConfigurationValue
    {
        public string Hostname { get; set; }
        public IDictionary<string, bool> PluginIsEnabled { get; set; }
    }  
}