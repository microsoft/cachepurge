/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;

    public class PartnerConfigRequest
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string Tenant { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string Name { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string ContactEmail { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string NotifyContactEmail { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public CdnConfiguration CdnConfiguration { get; set; }

        public override string ToString()
        {
            return $"{nameof(Tenant)}: {Tenant}, " +
                   $"{nameof(Name)}: {Name}, " +
                   $"{nameof(ContactEmail)}: {ContactEmail}, " +
                   $"{nameof(NotifyContactEmail)}: {NotifyContactEmail}, " +
                   $"{nameof(CdnConfiguration)}: {CdnConfiguration}";
        }
    }
}