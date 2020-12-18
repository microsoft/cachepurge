/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;

    public class AfdPartnerRequest : PartnerRequest
    {
        public string TenantID { get; set; }

        public string PartnerID { get; set; }

        public string Description { get; set; }
    }
}
