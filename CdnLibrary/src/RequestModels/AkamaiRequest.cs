/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System;
    using System.Text.Json.Serialization;

    public class AkamaiRequest : CosmosDbEntity, ICdnRequest
    {
        public AkamaiRequest() { }

        public AkamaiRequest(string partnerRequestID, string[] urls)
        {
            id = Guid.NewGuid().ToString();
            PartnerRequestID = partnerRequestID;
            Urls = urls;
        }

        public string PartnerRequestID { get; set; }

        public int NumTimesProcessed { get; set; }

        /// <summary>
        /// PurgeId returned by Akamai
        /// </summary>
        public string CdnRequestId { get; set; }

        /// <summary>
        /// Identifier to provide Akamai Technical Support if issues arise.
        /// </summary>
        public string SupportId { get; set; }

        public string Endpoint { get; set; }

        public string RequestBody { get; set; }

        public string Status { get; set; }

        public string[] Urls { get; set; }
    }

    internal class AkamaiRequestBody
    {
        [JsonPropertyName("objects")]
        public string[] Objects { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }
    }

    public class AkamaiRequestInfo : IRequestInfo
    {
        public string RequestID { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public string SupportID { get; set; }
    }

}
