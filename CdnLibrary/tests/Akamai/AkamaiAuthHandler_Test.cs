/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;

    [TestClass]
    public class AkamaiAuthHandler_Test
    {
        private readonly string clientToken = "testClientToken";
        private readonly string accessToken = "testAccessToken";
        private readonly string secret = "secret";

        private readonly string host = "testuri";

        [TestMethod]
        public void GetAuthData_Success()
        {
            string clientToken = "testClientToken";
            string accessToken = "testAccessToken";
            string secret = "secret";
            var timestamp = new DateTime(1918, 11, 11, 11, 00, 00, DateTimeKind.Utc).ToString("yyyyMMdd'T'HH:mm:ss+0000");

            var handler = new AkamaiAuthHandler(new HttpClientHandler(), clientToken, accessToken, secret);

            string authData = handler.GetAuthorizationData(timestamp);

            var reg = $"EG1-HMAC-SHA256 client_token={clientToken};access_token={accessToken};";
            reg += "timestamp=19181111T11:00:00\\+0000;nonce=[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12};";

            Assert.IsTrue(Regex.IsMatch(authData, reg));
        }

        [TestMethod]
        public void GetRequestData_Success()
        {
            string clientToken = "testClientToken";
            string accessToken = "testAccessToken";
            string secret = "secret";

            var host = "testuri";

            var handler = new AkamaiAuthHandler(new HttpClientHandler(), clientToken, accessToken, secret);

            using var req = new HttpRequestMessage(HttpMethod.Post, $"https://{host}/ccu/1")
            {
                Content = new StringContent("TestContent")
            };

            var data = $"POST\thttps\t{host}\t/ccu/1\t\tuY/AmsDfO7we5eeTFmBPdGL//fCVwcZ248JRd3NkX+k=\t";

            string reqData = handler.GetRequestData(req).Result;

            Assert.AreEqual(data, reqData);
        }

        [TestMethod]
        public void GetRequestData_NoContent()
        {
            string clientToken = "testClientToken";
            string accessToken = "testAccessToken";
            string secret = "secret";

            var host = "testuri";

            var handler = new AkamaiAuthHandler(new HttpClientHandler(), clientToken, accessToken, secret);

            using var req = new HttpRequestMessage(HttpMethod.Post, $"https://{host}/ccu/1");

            var data = $"POST\thttps\t{host}\t/ccu/1\t\t\t";

            string reqData = handler.GetRequestData(req).Result;

            Assert.AreEqual(data, reqData);
        }

        [TestMethod]
        public void GetRequestData_NoPath()
        {
            var handler = new AkamaiAuthHandler(new HttpClientHandler(), clientToken, accessToken, secret);

            using var req = new HttpRequestMessage(HttpMethod.Post, $"https://{host}")
            {
                Content = new StringContent("TestContent")
            };

            var data = $"POST\thttps\t{host}\t/\t\tuY/AmsDfO7we5eeTFmBPdGL//fCVwcZ248JRd3NkX+k=\t";

            string reqData = handler.GetRequestData(req).Result;

            Assert.AreEqual(data, reqData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AkamaiAuthHandler_NullHandler()
        {
            _ = new AkamaiAuthHandler(null, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AkamaiAuthHandler_Tokens()
        {
            _ = new AkamaiAuthHandler(new HttpClientHandler(), null, null, null);
        }

        [TestMethod]
        [Ignore]
        public void SendRequestToAkamai_Success()
        {
            string clientToken_ = "real client token";
            string accessToken_ = "real access token";
            string secret_ = "real secret";

            string endpoint = "https://fakeEndpoint";

            var urls = @"{""objects"": [""https://fakeUri""]}";
            var purgeRequest = new StringContent(urls, Encoding.UTF8, "application/json");

            using var httpHandler = new HttpHandler(new AkamaiAuthHandler(new HttpClientHandler(), clientToken_, accessToken_, secret_));

            var response = httpHandler.PostAsync(endpoint, purgeRequest).Result;

            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}
