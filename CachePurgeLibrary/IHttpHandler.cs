/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpHandler : IDisposable
    {
        Task<HttpResponseMessage> PostAsync(string endpoint, StringContent urls);

        Task<HttpResponseMessage> GetAsync(string endpoint);
    }
}
