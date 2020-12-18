/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    public interface ICdnRequest : ICosmosDbEntity
    {
        /// <summary>
        /// Id of the PartnerRequest this CdnRequest was batched from
        /// </summary>
        public string PartnerRequestID { get; set; }

        public string[] Urls { get; set; }

        public string Status { get; set; }

        public int NumTimesProcessed { get; set; }

        public string RequestBody { get; set; }

        public string Endpoint { get; set; }

        /// <summary>
        /// Id returned by CDN once cache purge is submitted
        /// </summary>
        public string CdnRequestId { get; set; }
    }
}
