/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    internal static class AkamaiRequestProcessor
    {
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("Akamai_ClientSecret") ?? "fakesecret";
        private static readonly string AccessToken = Environment.GetEnvironmentVariable("Akamai_AccessToken") ?? "faketoken";
        private static readonly string ClientToken = Environment.GetEnvironmentVariable("Akamai_ClientToken") ?? "faketoken";

        private static readonly ReadOnlyMemory<char> PurgeId = "purgeId".AsMemory();
        private static readonly ReadOnlyMemory<char> SupportId = "supportId".AsMemory();

        internal static async Task<AkamaiRequestInfo> SendPurgeRequest(string endpoint, StringContent purgeRequest, ILogger logger)
        {
            var akamaiAuthHandler = new AkamaiAuthHandler(new HttpClientHandler(), ClientToken, AccessToken, ClientSecret);

            using var httpHandler = new HttpHandler(akamaiAuthHandler);

            return await SendPurgeRequest(endpoint, purgeRequest, httpHandler, logger);
        }

        internal static async Task<AkamaiRequestInfo> SendPurgeRequest(string requestEndPoint, StringContent purgeRequest, IHttpHandler httpHandler, ILogger logger)
        {
            var requestInfo = new AkamaiRequestInfo()
            {
                RequestStatus = RequestStatus.Error
            };

            if (!string.IsNullOrEmpty(requestEndPoint) && purgeRequest != null)
            {
                try
                {
                    var response = await httpHandler.PostAsync(requestEndPoint, purgeRequest);

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(responseContent) &&
                        GetPropertyValue(PurgeId, responseContent, out var requestID) &&
                        GetPropertyValue(SupportId, responseContent, out var supportID))
                    {
                        requestInfo.RequestID = requestID;
                        requestInfo.RequestStatus = RequestStatus.PurgeCompleted;
                        requestInfo.SupportID = supportID;
                    }
                    else
                    {
                        logger.LogInformation($"AkamaiRequestProcessor: Purge Request not successful: {response.StatusCode}, {responseContent}");
                        requestInfo.RequestStatus = CdnPluginHelper.GetRequestStatusFromResponseCode(response.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"AkamaiRequestProcessor: Error while sending purge request: {e.Message}\n {e.StackTrace}");
                }
            }

            return requestInfo;
        }

        internal static bool GetPropertyValue(ReadOnlyMemory<char> propName, string response, out string propValue)
        {
            propValue = null;

            using JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty(propName.Span, out var idElement) && idElement.ValueKind == JsonValueKind.String)
            {
                propValue = idElement.ToString();
                return true;
            }

            return false;
        }
    }
}
