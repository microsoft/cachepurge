/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Threading.Tasks;

    public interface ICdnRequestTableManager<in T> : IDisposable where T : Enum
    {
        public Task CreateCdnRequest(ICdnRequest request, T cdn);

        public Task UpdateCdnRequest(ICdnRequest request, T cdn);
    }
}
