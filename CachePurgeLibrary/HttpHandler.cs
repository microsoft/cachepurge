/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HttpHandler : IHttpHandler
    {
        private readonly HttpClient httpClient;

        public HttpHandler() 
        { 
            this.httpClient = new HttpClient();
        }

        public HttpHandler(DelegatingHandler delegatingHandler)
        {
            this.httpClient = new HttpClient(delegatingHandler);
        }

        public Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return this.httpClient.GetAsync(endpoint);
        }

        public Task<HttpResponseMessage> PostAsync(string endpoint, StringContent urls)
        {
            return this.httpClient.PostAsync(endpoint, urls);
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}
