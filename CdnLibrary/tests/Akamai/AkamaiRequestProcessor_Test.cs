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
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class AkamaiRequestProcessor_Test
    {
        private readonly ILogger logger = Mock.Of<ILogger>();

        [TestMethod]
        public void SendPurgeRequest_ErrorNoEndpoint()
        {
            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest(string.Empty, null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPurgeRequest_ErrorNoContent()
        {
            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest("https://fakeUri", null, logger).Result;

            Assert.AreEqual(RequestStatus.Error, requestInfo.RequestStatus);
        }
        [TestMethod]
        public void SendPurgeRequest_SuccessSubmitted()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var purgeId = "12345";
            var supportId = "123";
            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(purgeId, supportId), logger).Result;

            Assert.AreEqual(RequestStatus.PurgeCompleted, requestInfo.RequestStatus);
            Assert.AreEqual(purgeId, requestInfo.RequestID);
            Assert.AreEqual(supportId, requestInfo.SupportID);
        }

        [TestMethod]
        public void SendPurgeRequest_UnknownNoRequestID()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(), logger).Result;

            Assert.AreEqual(RequestStatus.Unknown, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPurgeRequest_Throttled()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var id = "12345";
            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(id, statusCode: HttpStatusCode.TooManyRequests), logger).Result;

            Assert.AreEqual(RequestStatus.Throttled, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void SendPurgeRequest_Forbidden()
        {
            var purgeBody = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            const string id = "12345";
            var requestInfo = AkamaiRequestProcessor.SendPurgeRequest("https://fakeUri", purgeBody, GetHandler(id, statusCode: HttpStatusCode.Forbidden), logger).Result;
            Assert.AreEqual(RequestStatus.Forbidden, requestInfo.RequestStatus);
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_CorrectRequestID()
        {
            var response = @"{
  ""purgeId"": ""2230091"",
  ""supportId"": ""2230091""
}";
            Assert.IsTrue(AkamaiRequestProcessor.GetPropertyValue("purgeId".AsMemory(), response, out var purgeId));

            Assert.AreEqual("2230091", purgeId);

            Assert.IsTrue(AkamaiRequestProcessor.GetPropertyValue("supportId".AsMemory(), response, out var supportId));
            Assert.AreEqual("2230091", supportId);
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
            Assert.IsFalse(AkamaiRequestProcessor.GetPropertyValue("PurgeId".AsMemory(), response, out _));
            Assert.IsFalse(AkamaiRequestProcessor.GetPropertyValue("supportId".AsMemory(), response, out _));
        }

        [TestMethod]
        public void GetRequestStatusFromResponse_IncorrectFormatRequestIDAndStatus()
        {
            var response = @"{
   ""purgeId"": 2230091,
  ""supportId"": 2230091
}";
            Assert.IsFalse(AkamaiRequestProcessor.GetPropertyValue("PurgeId".AsMemory(), response, out _));
            Assert.IsFalse(AkamaiRequestProcessor.GetPropertyValue("supportId".AsMemory(), response, out _));
        }

        private static IHttpHandler GetHandler(string purgeId = null, string supportId = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var responseText = "{";

            if (!string.IsNullOrEmpty(purgeId))
            {
                responseText += @$"""purgeId"": ""{purgeId}""";
            }

            if (!string.IsNullOrEmpty(supportId))
            {
                responseText += @$", ""supportId"": ""{supportId}""";
            }

            responseText += "}";

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
