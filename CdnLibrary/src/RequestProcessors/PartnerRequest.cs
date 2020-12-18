/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System.Collections.Generic;

    public class PartnerRequest: CosmosDbEntity, IPartnerRequest
    {
        /// <summary>
        /// Id of the UserRequest this PartnerRequest was created from
        /// </summary>
        public string UserRequestID { get; set; }

        public string CDN { get; set; }

        public string Status { get; set; }

        public int NumTotalCdnRequests { get; set; }

        public int NumCompletedCdnRequests { get; set; }

        public ISet<string> Urls { get; set; }
    }
}