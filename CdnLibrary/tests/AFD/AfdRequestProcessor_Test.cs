/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class AfdRequestProcessor_Test
    {
        private readonly ILogger logger = Mock.Of<ILogger>();

        [TestMethod]
        public void SendPurgeRequest_ErrorNoEndpoint()
        {
            var requestInfo = AfdRequestProcessor.SendPurgeRequest(string.Empty, null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPurgeRequest_ErrorNoContent()
        {
            var requestInfo = AfdRequestProcessor.SendPurgeRequest("https://fakeUri", null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo.RequestStatus);
        }
        [TestMethod]
        public void SendPurgeRequest_SuccessSubmitted()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var id = "12345";
            var requestInfo = AfdRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(id), logger).Result;

            Assert.AreEqual(RequestStatus.PurgeSubmitted, requestInfo.RequestStatus);
            Assert.AreEqual(id, requestInfo.RequestID);
        }

        [TestMethod]
        public void SendPurgeRequest_UnknownNoRequestID()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var requestInfo = AfdRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(), logger).Result;

            Assert.AreEqual(RequestStatus.Unknown, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPurgeRequest_Throttled()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var id = "12345";
            var requestInfo = AfdRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(id, statusCode: HttpStatusCode.TooManyRequests), logger).Result;

            Assert.AreEqual(RequestStatus.Throttled, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPollRequest_ErrorNoEndpoint()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest(string.Empty, null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_ErrorNoContent()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_SuccessSubmitted()
        {
            var id = "12345";
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", id, GetHandler(id, "RolledOut"), logger).Result;

            Assert.AreEqual(RequestStatus.PurgeCompleted, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_SuccessNotStarted()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", "12345", GetHandler(status: "NotStarted"), logger).Result;

            Assert.AreEqual(RequestStatus.PurgeSubmitted, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_Unauthorized()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", "12345", GetHandler(statusCode: HttpStatusCode.Unauthorized), logger).Result;

            Assert.AreEqual(RequestStatus.Unauthorized, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_Error()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", "12345", GetHandler(statusCode: HttpStatusCode.InternalServerError), logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo);
        }

        [TestMethod]
        public void SendPollRequest_Throttled()
        {
            var requestInfo = AfdRequestProcessor.SendPollRequest("https://fakeUri", "12345", GetHandler(statusCode: HttpStatusCode.TooManyRequests), logger).Result;

            Assert.AreEqual(RequestStatus.Throttled, requestInfo);
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_CorrectRequestID()
        {
            var response = @"{
  ""Id"": 2230090,
  ""Description"": ""string"",
  ""Urls"": [
    ""https://fakeUri?pid=1.1""
  ],
  ""CreateTime"": ""0001 -01-01T00:00:00"",
  ""RequestUser"": ""shishar@microsoft.com"",
  ""CompleteTime"": null,
  ""Status"": ""NotStarted"",
  ""PercentComplete"": 0
}";
            Assert.IsTrue(AfdRequestProcessor.GetRequestID(response, out var id));

            Assert.AreEqual("2230090", id);
            var status = AfdRequestProcessor.GetRequestStatusFromContent(response);
            Assert.AreEqual(RequestStatus.PurgeSubmitted, status);
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_NoRequestIDAndStatus()
        {
            var response = @"{
  ""Description"": ""string"",
  ""Urls"": [
    ""https://fakeUri?pid=1.1""
  ],
  ""CreateTime"": ""0001 -01-01T00:00:00"",
  ""RequestUser"": ""shishar@microsoft.com"",
  ""CompleteTime"": null,
  ""PercentComplete"": 0
}";
            Assert.IsFalse(AfdRequestProcessor.GetRequestID(response, out _));

            var status = AfdRequestProcessor.GetRequestStatusFromContent(response);
            Assert.AreEqual(RequestStatus.Unknown, status);
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_IncorrectFormatRequestIDAndStatus()
        {
            var response = @"{
  ""Id"": ""test"",
  ""Description"": ""string"",
  ""Urls"": [
    ""https://fakeUri?pid=1.1""
  ],
  ""CreateTime"": ""0001 -01-01T00:00:00"",
  ""RequestUser"": ""shishar@microsoft.com"",
  ""CompleteTime"": null,
  ""Status"": 1,
  ""PercentComplete"": 0
}";
            Assert.IsFalse(AfdRequestProcessor.GetRequestID(response, out _));

            var status = AfdRequestProcessor.GetRequestStatusFromContent(response);
            Assert.AreEqual(RequestStatus.Unknown, status);
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_NoStatus()
        {
            var response = @"{
  ""Id"": ""test"",
  ""Description"": ""string"",
  ""Urls"": [
    ""https://fakeUri?pid=1.1""
  ],
  ""CreateTime"": ""0001 -01-01T00:00:00"",
  ""RequestUser"": ""shishar@microsoft.com"",
  ""CompleteTime"": null,
  ""PercentComplete"": 0
}";
            Assert.IsFalse(AfdRequestProcessor.GetRequestID(response, out _));

            var status = AfdRequestProcessor.GetRequestStatusFromContent(response);
            Assert.AreEqual(RequestStatus.Unknown, status);
        }

        private static IHttpHandler GetHandler(string requestID = null, string status = "NotStarted", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var responseText = @$"{{
  ""Description"": ""string"",
  ""Urls"": [
    ""https://fakeUri?pid=1.1""
  ],
  ""CreateTime"": ""0001 - 01 - 01T00: 00:00"",
  ""RequestUser"": ""shishar @microsoft.com"",
  ""CompleteTime"": null,
  ""Status"": ""{status}"",
  ""PercentComplete"": 0";

            if (!string.IsNullOrEmpty(requestID))
            {
                responseText += @$", ""Id"": {requestID}";
            }

            responseText += @"}";

            var response = new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent(responseText)
            };

            var handler = new Mock<IHttpHandler>();
            handler.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>())).Returns(Task.FromResult(response));
            handler.Setup(h => h.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(response));

            return handler.Object;
        }
    }
}
