/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnPlugin
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class CdnPlugin_Test
    {
        private readonly IDictionary<string, AfdPartnerRequest> afdPartnerRequest = new Dictionary<string, AfdPartnerRequest>();
        private readonly IDictionary<string, AkamaiPartnerRequest> akamaiPartnerRequest = new Dictionary<string, AkamaiPartnerRequest>();

        private readonly List<ICdnRequest> OutputDB = new List<ICdnRequest>();

        private static readonly string tenantId = "FakeTenant";
        private static readonly string partnerId = "FakePartner";

        private static readonly string[] urls = new string[] { "https://fakeUrls" };

        CdnPluginFunctions cdnPluginFunctions;

        [TestInitialize]
        public void Setup()
        {
            var afdPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(afdPartnerRequest);
            var akamaiPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(akamaiPartnerRequest);

            var partnerRequestTable = new PartnerRequestTableManager(afdPartnerRequestContainer, akamaiPartnerRequestContainer);

            cdnPluginFunctions = new CdnPluginFunctions(partnerRequestTable);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void PurgeAfd_Fail_Empty()
        {
            IReadOnlyList<Document> doc = new List<Document>();

            cdnPluginFunctions.PurgeAfd(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(0, OutputDB.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void PurgeAkamai_Fail_Empty()
        {
            IReadOnlyList<Document> doc = new List<Document>();

            cdnPluginFunctions.PurgeAkamai(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(0, OutputDB.Count);
        }

        [TestMethod]
        public void PurgeAfd_Success()
        {
            var id = Guid.NewGuid().ToString();
            var partnerDoc = new Document();
            partnerDoc.SetPropertyValue("CDN", "AFD");
            partnerDoc.SetPropertyValue("tenantId", tenantId);
            partnerDoc.SetPropertyValue("partnerId", partnerId);
            partnerDoc.SetPropertyValue("Urls", urls);
            partnerDoc.SetPropertyValue("id", id);

            IReadOnlyList<Document> doc = new List<Document>()
            {
                partnerDoc
            };

            cdnPluginFunctions.PurgeAfd(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(1, OutputDB.Count);
            Assert.AreEqual(id, OutputDB[0].PartnerRequestID);
        }

        [TestMethod]
        public void PurgeAkamai_Success()
        {
            var id = Guid.NewGuid().ToString();

            var partnerDoc = new Document();
            partnerDoc.SetPropertyValue("CDN", "Akamai");
            partnerDoc.SetPropertyValue("tenantId", tenantId);
            partnerDoc.SetPropertyValue("partnerId", partnerId);
            partnerDoc.SetPropertyValue("Urls", urls);
            partnerDoc.SetPropertyValue("id", id);

            IReadOnlyList<Document> doc = new List<Document>()
            {
                partnerDoc
            };

            cdnPluginFunctions.PurgeAkamai(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(1, OutputDB.Count);
            Assert.AreEqual(id, OutputDB[0].PartnerRequestID);
        }

        [TestMethod]
        public void PurgeAfd_Fail_NoId()
        {
            var partnerDoc = new Document();
            partnerDoc.SetPropertyValue("CDN", "Afd");
            partnerDoc.SetPropertyValue("tenantId", tenantId);
            partnerDoc.SetPropertyValue("partnerId", partnerId);
            partnerDoc.SetPropertyValue("Urls", urls);

            IReadOnlyList<Document> doc = new List<Document>()
            {
                partnerDoc
            };

            cdnPluginFunctions.PurgeAfd(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(0, OutputDB.Count);
        }

        [TestMethod]
        public void PurgeAkamai_Fail_NoId()
        {
            var partnerDoc = new Document();
            partnerDoc.SetPropertyValue("CDN", "Akamai");
            partnerDoc.SetPropertyValue("tenantId", tenantId);
            partnerDoc.SetPropertyValue("partnerId", partnerId);
            partnerDoc.SetPropertyValue("Urls", urls);

            IReadOnlyList<Document> doc = new List<Document>()
            {
                partnerDoc
            };

            cdnPluginFunctions.PurgeAkamai(doc, CreateCollector(), Mock.Of<ILogger>()).Wait();
            Assert.AreEqual(0, OutputDB.Count);
        }

        private ICollector<ICdnRequest> CreateCollector()
        {
            var collector = new Mock<ICollector<ICdnRequest>>();

            collector.Setup(c => c.Add(It.IsAny<ICdnRequest>())).Callback<ICdnRequest>((s) => OutputDB.Add(s));

            return collector.Object;
        }
    }
}
