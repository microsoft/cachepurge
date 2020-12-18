/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Threading.Tasks;

    public interface IPartnerRequestTableManager<in T> : IDisposable where T : Enum
    {
        public Task CreatePartnerRequest(IPartnerRequest partnerRequest, T cdn);

        public Task UpdatePartnerRequest(IPartnerRequest partnerRequest, T cdn);

        public Task<IPartnerRequest> GetPartnerRequest(string id, T cdn);
    }
}
