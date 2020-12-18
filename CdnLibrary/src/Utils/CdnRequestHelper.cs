/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System;

    internal static class CdnRequestHelper
    {
        public static IPartnerRequest CreatePartnerRequest(CDN cdn, Partner partner, UserRequest userRequest, string description, string ticketId)
        {
            IPartnerRequest partnerRequest;

            if (cdn == CDN.Akamai)
            {
                partnerRequest = new AkamaiPartnerRequest();
            }
            else if (cdn == CDN.AFD)
            {
                partnerRequest = new AfdPartnerRequest
                {
                    PartnerID = partner.Name,
                    TenantID = partner.TenantId,
                    Description = description + (string.IsNullOrEmpty(ticketId) ? "" : $" ({ticketId})")
                };
            }
            else
            {
                throw new ArgumentException($"We encountered an unknown CDN: {cdn}");
            }
            partnerRequest.id = Guid.NewGuid().ToString();
            partnerRequest.CDN = cdn.ToString();
            partnerRequest.Urls = userRequest.Urls;
            partnerRequest.UserRequestID = userRequest.id.ToString();

            return partnerRequest;
        }
    }

    public enum CDN
    {
        AFD = 1,
        Akamai = 2
    }
}
