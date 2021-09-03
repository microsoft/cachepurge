/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;

    public class Partner : CosmosDbEntity
    {
        public string TenantId { get; set; }

        public string Name { get; set; }

        // public string ContactEmail { get; set; }
        //
        // public string NotifyContactEmail { get; set; }

        public CdnConfiguration CdnConfiguration { get; set; }
        public string Hostname { get; set; }

        public Partner(string tenant, string name, /*string contactEmail, string notifyContactEmail,*/
            CdnConfiguration cdnConfiguration)
        {
            id = Guid.NewGuid().ToString();
            Name = name;
            TenantId = tenant;
            // ContactEmail = contactEmail;
            // NotifyContactEmail = notifyContactEmail;
            CdnConfiguration = cdnConfiguration;
        }
    }
}