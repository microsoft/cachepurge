/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;

    public class PartnerConfigRequest
    {
        public string Tenant { get; set; }

        public string Name { get; set; }

        public CdnConfiguration CdnConfiguration { get; set; }

        public override string ToString()
        {
            return $"{nameof(Tenant)}: {Tenant}, " +
                   $"{nameof(Name)}: {Name}, " +
                   $"{nameof(CdnConfiguration)}: {CdnConfiguration}";
        }
    }
}