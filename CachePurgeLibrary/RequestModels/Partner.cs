/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Collections.Generic;

    public class Partner : CosmosDbEntity
    {
        public string TenantId { get; set; }

        public string Name { get; set; }

        public string ContactEmail { get; set; }

        public string NotifyContactEmail { get; set; }

        public IEnumerable<CdnConfiguration> CdnConfigurations { get; set; }

        public Partner(string tenant, string name, string contactEmail, string notifyContactEmail,
            CdnConfiguration[] cdnConfigurations)
        {
            id = Guid.NewGuid().ToString();
            Name = name;
            TenantId = tenant;
            ContactEmail = contactEmail;
            NotifyContactEmail = notifyContactEmail;
            CdnConfigurations = cdnConfigurations;
        }
    }
}