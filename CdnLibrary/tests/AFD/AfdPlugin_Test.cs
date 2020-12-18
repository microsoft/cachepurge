/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class AfdPlugin_Test
    {
        private static readonly string tenantId = "FakeTenant";
        private static readonly string partnerId = "FakePartner";
        private static readonly string id = Guid.NewGuid().ToString();

        private readonly List<ICdnRequest> OutputQueue = new List<ICdnRequest>();
        private readonly AfdPlugin plugin = new AfdPlugin(Mock.Of<ILogger>(), 500);

        [TestMethod]
        public void ProcessPartnerRequest_NullPartnerAndTenantID()
        {
            string json =
                @"{
""CDN"": ""AFD"",
""tenantId"": """",
""partnerId"": """",
""Urls"": [""https://fakeUri?pid=1.1""]}";

            Assert.IsFalse(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsFalse(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_HasPartnerAndTenantID()
        {
            string json =
                @$"{{
""CDN"": ""AFD"",
""tenantId"": ""{tenantId}"",
""partnerId"": ""{partnerId}"",
""id"": ""{id}"",
""Urls"": [""https://fakeUri?pid=1.1""]}}";

            Assert.IsTrue(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(1, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_NoUrl()
        {
            string json =
                @$"{{
""CDN"": ""AFD"",
""tenantId"": ""{tenantId}"",
""partnerId"": ""{partnerId}"",
""id"": ""{id}""
}}";

            Assert.IsFalse(plugin.ValidPartnerRequest(json, "1", out var p));

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_MaxUrl()
        {
            string json =
                @$"{{
""CDN"": ""AFD"",
""id"": ""{id}"",
""tenantId"": ""{tenantId}"",
""partnerId"": ""{partnerId}"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.2"",
""https://fakeUri?pid=1.3""]}}";

            var plugin2 = new AfdPlugin(Mock.Of<ILogger>(), 2);
            Assert.IsTrue(plugin2.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin2.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(2, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_BatchCreated()
        {
            string json =
                @$"{{
""CDN"": ""AFD"",
""Status"": ""BatchCreated"",
""id"": ""{id}"",
""tenantId"": ""{tenantId}"",
""partnerId"": ""{partnerId}"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.2"",
""https://fakeUri?pid=1.3""]}}";

            Assert.IsFalse(plugin.ValidPartnerRequest(json, "1", out var p));

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_DoesNotAddDuplicate()
        {
            var desc = "UnitTest";
            string json =
                @$"{{
""CDN"": ""AFD"",
""tenantId"": ""{tenantId}"",
""partnerId"": ""{partnerId}"",
""id"": ""{id}"",
""Description"": ""{desc}"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.1"",
""https://fakeUri?pid=1.1""]}}";

            Assert.IsTrue(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(1, OutputQueue.Count);

            var req = OutputQueue[0] as AfdRequest;

            Assert.AreEqual(tenantId, req.TenantID);
            Assert.AreEqual(partnerId, req.PartnerID);
            Assert.AreEqual(desc, req.Description);
        }

        private ICollector<ICdnRequest> CreateCollector()
        {
            var collector = new Mock<ICollector<ICdnRequest>>();

            collector.Setup(c => c.Add(It.IsAny<ICdnRequest>())).Callback<ICdnRequest>((s) => OutputQueue.Add(s));

            return collector.Object;
        }
    }
}
