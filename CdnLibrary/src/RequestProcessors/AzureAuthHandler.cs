/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Message handler that gets an auth token using client credentials (AppId) based auth from Azure
    /// active directory and adds the appropriate header for Authorization to the request
    /// </summary>
    public class AzureAuthHandler : DelegatingHandler
    {
        private readonly string loginUrl;
        private readonly string resource;
        private readonly string appId;
        private readonly string appKey; //from KeyVault

        public AzureAuthHandler(
            HttpMessageHandler innerContent,
            string resource,
            string loginUrl,
            string appId,
            string appKey)
            : base(innerContent)
        {
            this.resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.loginUrl = loginUrl ?? throw new ArgumentNullException(nameof(loginUrl));
            this.appId = appId ?? throw new ArgumentNullException(nameof(appId));
            this.appKey = appKey ?? throw new ArgumentNullException(nameof(appKey));
        }

        /// <summary>
        /// This message handler gets a token from AAD and adds it as an auth header to original request
        /// and submits original request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var authContext = new AuthenticationContext(this.loginUrl);
            ClientCredential clientCred = new ClientCredential(this.appId, this.appKey);
            var authResult = await authContext.AcquireTokenAsync(this.resource, clientCred);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                authResult.AccessTokenType,
                authResult.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}