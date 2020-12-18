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
    public class AkamaiPlugin_Test
    {
        private readonly List<ICdnRequest> OutputQueue = new List<ICdnRequest>();

        private readonly AkamaiPlugin plugin = new AkamaiPlugin(Mock.Of<ILogger>(), 500);

        [TestMethod]
        public void ProcessPartnerRequest_NoNetwork()
        {
            string json =
                @"{
""CDN"": ""Akamai"",
""id"": ""1"",
""Urls"": [""https://fakeUri?pid=1.1""]}";

            Assert.IsTrue(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(1, OutputQueue.Count);
            Assert.IsTrue(OutputQueue[0].Endpoint.EndsWith("staging"));
        }

        [TestMethod]
        public void ProcessPartnerRequest_InvalidNetworkValue()
        {
            string json =
                @$"{{
""CDN"": ""Akamai"",
""id"": ""1"",
""Network"": ""Fake"",
""Urls"": [""https://fakeUri?pid=1.1""]}}";

            Assert.IsTrue(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(1, OutputQueue.Count);
            Assert.IsTrue(OutputQueue[0].Endpoint.EndsWith("staging"));
        }

        [TestMethod]
        public void ProcessPartnerRequest_NoUrl()
        {
            string json =
                @$"{{
""CDN"": ""Akamai"",
""id"": ""1""
}}";

            Assert.IsFalse(plugin.ValidPartnerRequest(json, "1", out var _));

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_MaxUrl()
        {
            string json =
                @$"{{
""CDN"": ""Akamai"",
""id"": ""1"",
""Network"": ""Production"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.2"",
""https://fakeUri?pid=1.3""]}}";

            var plugin2 = new AkamaiPlugin(Mock.Of<ILogger>(), 2);
            Assert.IsTrue(plugin2.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin2.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(2, OutputQueue.Count);
            Assert.IsTrue(OutputQueue[0].Endpoint.EndsWith("production"));
        }

        [TestMethod]
        public void ProcessPartnerRequest_BatchCreated()
        {
            string json =
                @$"{{
""CDN"": ""Akamai"",
""id"": ""1"",
""Status"": ""BatchCreated""
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.2"",
""https://fakeUri?pid=1.3""]}}";

            Assert.IsFalse(plugin.ValidPartnerRequest(json, "1", out var _));

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessPartnerRequest_DoesNotAddDuplicate()
        {
            string json =
                @$"{{
""CDN"": ""Akamai"",
""id"": ""1"",
""Urls"": [""https://fakeUri?pid=1.1"", 
""https://fakeUri?pid=1.1"",
""https://fakeUri?pid=1.1""]}}";

            Assert.IsTrue(plugin.ValidPartnerRequest(json, "1", out var p));
            Assert.IsTrue(plugin.ProcessPartnerRequest(p, queue: CreateCollector()));

            Assert.AreEqual(1, OutputQueue.Count);
        }

        private ICollector<ICdnRequest> CreateCollector()
        {
            var collector = new Mock<ICollector<ICdnRequest>>();

            collector.Setup(c => c.Add(It.IsAny<ICdnRequest>())).Callback<ICdnRequest>((s) => OutputQueue.Add(s));

            return collector.Object;
        }
    }
}
