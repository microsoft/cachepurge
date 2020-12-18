/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System;

    public class AfdRequest : CosmosDbEntity, ICdnRequest
    {
        public AfdRequest() { }

        public AfdRequest(string partnerRequestID, string tenantId, string partnerId, string description, string[] urls)
        {
            this.id = Guid.NewGuid().ToString();
            PartnerRequestID = partnerRequestID;
            TenantID = tenantId;
            PartnerID = partnerId;
            Description = description;
            Urls = urls;
        }

        public string CdnRequestId { get; set; }

        public string PartnerRequestID { get; set; }

        public string TenantID { get; set; }

        public string PartnerID { get; set; }

        public string Description { get; set; }

        public string[] Urls { get; set; }

        public string Status { get; set; } = string.Empty;

        public int NumTimesProcessed { get; set; }

        public string Endpoint { get; set; }

        public string RequestBody { get; set; }
    }

    internal class AfdRequestBody
    {
        public string Description { get; set; }

        public string[] Urls { get; set; }
    }

    public class AfdRequestInfo : IRequestInfo
    {
        public string RequestID { get; set; }
        public RequestStatus RequestStatus { get; set; }
    }

}
