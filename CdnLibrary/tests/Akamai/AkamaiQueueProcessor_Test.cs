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
    using System.Collections.Generic;
    using System.Text.Json;

    [TestClass]
    public class AkamaiQueueProcessor_Test
    {
        private readonly List<ICdnRequest> OutputDB = new List<ICdnRequest>();
        private readonly AkamaiQueueProcessor processor = new AkamaiQueueProcessor(Mock.Of<ILogger>());

        [TestMethod]
        public void ProcessPurgeRequest_EmptyQueueMessage()
        {
            var msg = processor.ProcessPurgeRequest(new AkamaiRequest(), 5).Result;

            Assert.IsNull(msg);
            Assert.AreEqual(0, OutputDB.Count);
        }

        [TestMethod]
        public void ProcessPurgeRequest_QueueMessage()
        {
            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                CdnRequestId = "100",
                RequestBody = "testBody",
                Urls = new string[]
                {
                    "https://fakeUri?pid=1.1",
                }
            };

            var msg = processor.ProcessPurgeRequest(queueMsg, 5).Result;

            Assert.IsNotNull(msg);
            Assert.AreEqual(1, queueMsg.NumTimesProcessed);
            Assert.AreEqual(0, OutputDB.Count);
        }

        [TestMethod]
        public void ProcessPurgeRequest_MaxRetry()
        {
            string json =
                @"{
""CDN"": ""AFD"",
""tenantId"": ""1"",
""partnerId"": ""1"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.1"",
""https://fakeUri?pid=1.1""]}";

            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 5,
                RequestBody = json,
                Urls = new string[]
                {
                    "https://fakeUri?pid=1.1",
                },
                Status = RequestStatus.PurgeSubmitted.ToString()
            };

            var msg = processor.ProcessPurgeRequest(queueMsg, 5).Result;

            Assert.IsNull(msg);
            Assert.AreEqual(5, queueMsg.NumTimesProcessed);
            Assert.IsTrue(CdnQueueHelper.AddCdnRequestToDB(queueMsg, 5));
            Assert.AreEqual(RequestStatus.MaxRetry.ToString(), queueMsg.Status);
        }

        [TestMethod]
        public void ProcessPurgeRequest_SuccessDontCreatePollMsg()
        {
            var requestId = "12345";
            var supportId = "testID";

            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 1,
                Urls = new string[]
                {
                    "https://fakeUri?pid=1.1",
                }
            };

            var requestInfo = new AkamaiRequestInfo()
            {
                RequestID = requestId,
                RequestStatus = RequestStatus.PurgeCompleted,
                SupportID = supportId
            };

            var msg = processor.CompletePurgeRequest(requestInfo, queueMsg);

            Assert.IsNull(msg);
            Assert.AreEqual(requestId, queueMsg.CdnRequestId);
            Assert.AreEqual(supportId, queueMsg.SupportId);
        }

        [TestMethod]
        public void ProcessPurgeRequest_ThrottledIncreaseNumTimesProcessed()
        {
            var requestId = "12345";

            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 1
            };

            var requestInfo = new AkamaiRequestInfo()
            {
                RequestID = requestId,
                RequestStatus = RequestStatus.Throttled
            };

            var msg = processor.CompletePurgeRequest(requestInfo, queueMsg);
            var pollMsg = JsonSerializer.Deserialize<AkamaiRequest>(msg.AsString);

            Assert.IsNotNull(msg);
            Assert.AreEqual(2, pollMsg.NumTimesProcessed);
            Assert.AreEqual(null, pollMsg.CdnRequestId);
        }

        [TestMethod]
        public void ProcessPurgeRequest_ErrorIncreaseNumTimesProcessed()
        {
            var requestId = "12345";

            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 1
            };

            var requestInfo = new AkamaiRequestInfo()
            {
                RequestID = requestId,
                RequestStatus = RequestStatus.Error
            };

            var msg = processor.CompletePurgeRequest(requestInfo, queueMsg);
            var pollMsg = JsonSerializer.Deserialize<AkamaiRequest>(msg.AsString);

            Assert.IsNotNull(msg);
            Assert.AreEqual(2, pollMsg.NumTimesProcessed);
            Assert.AreEqual(null, pollMsg.CdnRequestId);
        }

        [TestMethod]
        public void ProcessPurgeRequest_UnknownUnauthorizedReturnNull()
        {
            var requestId = "12345";

            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 1
            };

            var requestInfo = new AkamaiRequestInfo()
            {
                RequestID = requestId,
                RequestStatus = RequestStatus.Unauthorized
            };

            var msg = processor.CompletePurgeRequest(requestInfo, queueMsg);
            Assert.IsNull(msg);

            requestInfo = new AkamaiRequestInfo()
            {
                RequestID = requestId,
                RequestStatus = RequestStatus.Unknown
            };

            msg = processor.CompletePurgeRequest(requestInfo, queueMsg);

            Assert.IsNull(msg);
        }

        [TestMethod]
        public void AddCdnRequestToDB_ThrottledError()
        {
            var queueMsg = new AkamaiRequest()
            {
                Endpoint = "http://testendpoint",
                NumTimesProcessed = 1,
                Status = RequestStatus.Throttled.ToString()
            };

            Assert.IsFalse(CdnQueueHelper.AddCdnRequestToDB(queueMsg, 5));

            queueMsg.Status = RequestStatus.Error.ToString();

            Assert.IsFalse(CdnQueueHelper.AddCdnRequestToDB(queueMsg, 5));
        }
    }
}
