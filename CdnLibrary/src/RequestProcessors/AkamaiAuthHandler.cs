/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal class AkamaiAuthHandler : DelegatingHandler
    {
        private const string HashType = "SHA256";

        private readonly string clientToken;
        private readonly string accessToken;
        private readonly string clientSecret;
        private readonly KeyedHash hash;

        internal List<string> HeadersToInclude { get; private set; }

        public AkamaiAuthHandler(
            HttpMessageHandler innerHandler,
            string clientToken,
            string accessToken,
            string clientSecret)
            : base(innerHandler)
        {
            this.clientToken = clientToken ?? throw new ArgumentNullException(nameof(clientToken));
            this.accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            this.clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));

            this.hash = KeyedHash.HMACSHA256;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = await CreateAuthorizationHeader(request);

            request.Headers.Add(HeaderNames.Authorization, authHeader);

            return await base.SendAsync(request, cancellationToken);
        }

        public async Task<string> CreateAuthorizationHeader(HttpRequestMessage request)
        {
            if (request == null) 
            {
                throw new ArgumentNullException(nameof(request));
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HH:mm:ss+0000");
            var requestData = await GetRequestData(request);

            return GetAuthorizationHeader(timestamp, requestData);
        }

        internal string GetAuthorizationData(string timestamp)
        {
            var nonce = Guid.NewGuid();
            return
                $"{hash.Name} client_token={clientToken};" +
                $"access_token={accessToken};" +
                $"timestamp={timestamp};" +
                $"nonce={nonce.ToString().ToLowerInvariant()};";
        }

        internal async Task<string> GetRequestData(HttpRequestMessage request)
        {
            var bodyStream = request.Content != null ? await request.Content.ReadAsByteArrayAsync() : null;

            var requestHash = CdnPluginHelper.ComputeHash(bodyStream.AsSpan(), HashType);

            return $"POST\t{request.RequestUri.Scheme}\t" +
                $"{request.RequestUri.Host}\t{request.RequestUri.PathAndQuery}\t{string.Empty}\t{requestHash}\t";
        }

        internal string GetAuthorizationHeader(string timestamp, string requestData)
        {
            var authData = GetAuthorizationData(timestamp);

            var signingKey = CdnPluginHelper.ComputeKeyedHash(timestamp, clientSecret, hash.Algorithm);
            var authSignature = CdnPluginHelper.ComputeKeyedHash(requestData + authData, signingKey, hash.Algorithm);

            return $"{authData}signature={authSignature}";
        }

        internal struct KeyedHash
        {
            public static readonly KeyedHash HMACSHA256 = new KeyedHash() 
            {
                Name = "EG1-HMAC-SHA256", 
                Algorithm = "HMACSHA256"
            };

            public string Name { get; private set; }

            public string Algorithm { get; private set; }
        }
    }
}
