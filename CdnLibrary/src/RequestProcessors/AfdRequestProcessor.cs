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

    internal static class AfdRequestProcessor
    {
        private const string RolledOut = "RolledOut";

        private static readonly string LoginUrl = Environment.GetEnvironmentVariable("Afd_LoginUrl") ?? "https://fakeUrl";
        private static readonly string Resource = Environment.GetEnvironmentVariable("Afd_Resource") ?? "https://fakeResource";
        private static readonly string AppId = Environment.GetEnvironmentVariable("Afd_AppId") ?? "fakeAppId";
        private static readonly string AppKey = Environment.GetEnvironmentVariable("Afd_Secret") ?? "fakeSecret";

        private static readonly ReadOnlyMemory<char> Id = "Id".AsMemory();
        private static readonly ReadOnlyMemory<char> Status = "Status".AsMemory();

        internal static async Task<AfdRequestInfo> SendPurgeRequest(string afdEndpoint, StringContent purgeRequest, ILogger logger)
        {
            var azureAuthHandler = new AzureAuthHandler(new HttpClientHandler(), Resource, LoginUrl, AppId, AppKey);

            using var httpHandler = new HttpHandler(azureAuthHandler);

            return await SendPurgeRequest(afdEndpoint, purgeRequest, httpHandler, logger);
        }

        internal static async Task<RequestStatus> SendPollRequest(string afdEndpoint, string requestId, ILogger logger)
        {
            var azureAuthHandler = new AzureAuthHandler(new HttpClientHandler(), Resource, LoginUrl, AppId, AppKey);

            using var httpHandler = new HttpHandler(azureAuthHandler);

            return await SendPollRequest(afdEndpoint, requestId, httpHandler, logger);
        }

        internal static async Task<RequestStatus> SendPollRequest(string afdEndpoint, string requestId, IHttpHandler httpHandler, ILogger logger)
        {
            var requestStatus = RequestStatus.Error;

            if (!string.IsNullOrEmpty(afdEndpoint) && !string.IsNullOrEmpty(requestId))
            {
                try
                {
                    var response = await httpHandler.GetAsync($"{afdEndpoint}/{requestId}");

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(responseContent))
                    {
                        requestStatus = GetRequestStatusFromContent(responseContent);
                    }
                    else
                    {
                        requestStatus = CdnPluginHelper.GetRequestStatusFromResponseCode(response.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"AfdRequestProcessor: Error while sending poll request: {e.Message}\n {e.StackTrace}");
                }
            }

            return requestStatus;
        }

        internal static async Task<AfdRequestInfo> SendPurgeRequest(string requestEndPoint, StringContent purgeRequest, IHttpHandler httpHandler, ILogger logger)
        {
            var requestInfo = new AfdRequestInfo()
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
                        GetRequestID(responseContent, out var requestID))
                    {
                        requestInfo.RequestID = requestID;
                        requestInfo.RequestStatus = RequestStatus.PurgeSubmitted;
                    }
                    else
                    {
                        logger.LogInformation($"AfdRequestProcessor: Purge Request not successful: {response.StatusCode}, {responseContent}");
                        requestInfo.RequestStatus = CdnPluginHelper.GetRequestStatusFromResponseCode(response.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"AfdRequestProcessor: Error while sending purge request: {e.Message}\n {e.StackTrace}");
                }
            }

            return requestInfo;
        }

        internal static bool GetRequestID(string response, out string requestID)
        {
            requestID = null;

            using JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty(Id.Span, out var idElement) && idElement.ValueKind == JsonValueKind.Number &&
                idElement.TryGetInt64(out var ID))
            {
                requestID = ID.ToString();
                return true;
            }

            return false;
        }

        internal static RequestStatus GetRequestStatusFromContent(string result)
        {
            if (GetStringProperty(result, Status, out var status))
            {
                if (RolledOut.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    return RequestStatus.PurgeCompleted;
                }

                return RequestStatus.PurgeSubmitted;
            }

            return RequestStatus.Unknown;
        }

        private static bool GetStringProperty(string response, ReadOnlyMemory<char> propertyName, out string propertyValue)
        {
            propertyValue = null;

            using JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty(propertyName.Span, out var propVal) && propVal.ValueKind == JsonValueKind.String)
            {
                propertyValue = propVal.ToString();
                return true;
            }

            return  false;
        }
    }

    
}
