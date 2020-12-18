/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PartnersFunctions_Test
    {
        private PartnerFunctions partnerFunctions;
        private IRequestTable<Partner> partnerTable;

        private const string TenantId = "FakeTenant";
        private const string Name = "FakePartner";
        private const string DriContact = "driContact@example.test";
        private const string NotifyContact = "notifyContact@example.test";
        private const string TestHostname = "test_hostname";

        private readonly Dictionary<string, Partner> partners = new Dictionary<string, Partner>();

        [TestInitialize]
        public void Setup()
        {
            partnerTable = new PartnerTable(CdnLibraryTestHelper.MockCosmosDbContainer(partners));
            partnerFunctions = new PartnerFunctions(partnerTable);
        }

        [TestMethod]
        public void TestCreatePartner_Success()
        {
            var testPartnerResponse = CreateTestPartner();

            Assert.AreEqual(typeof(StringResult), testPartnerResponse.GetType());
            var partnerGuid = ((StringResult) testPartnerResponse).Value.ToString();
            Assert.IsTrue(partners.Count > 0);
            var partner = partnerTable.GetItem(partnerGuid).Result;

            TestPartner(partner);
        }

        [TestMethod]
        public void TestCreatePartner_Fail()
        {
            var createPartnerResponse = partnerFunctions.CreatePartner(new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    ["Test"] = "Bad"
                }
            )}, null).Result;

            Assert.AreEqual(typeof(ExceptionResult), createPartnerResponse.GetType());
        }

        private static void TestPartner(Partner partner)
        {
            Assert.AreEqual(TenantId, partner.TenantId);
            Assert.AreEqual(Name, partner.Name);
            Assert.AreEqual(DriContact, partner.ContactEmail);
            Assert.AreEqual(NotifyContact, partner.NotifyContactEmail);
            var partnerCdnConfigurations = partner.CdnConfigurations.ToList();
            Assert.AreEqual(1, partnerCdnConfigurations.Count);
            Assert.AreEqual(TestHostname, partnerCdnConfigurations[0].Hostname);
            var cdnWithCredentials = partnerCdnConfigurations[0].CdnWithCredentials;
            Assert.AreEqual(2, cdnWithCredentials.Count);
            Assert.AreEqual("", cdnWithCredentials[CDN.AFD.ToString()]);
            Assert.AreEqual("", cdnWithCredentials[CDN.Akamai.ToString()]);
        }


        
        private static void TestPartnerSerialization(JsonResult partner)
        {
            Assert.AreEqual(typeof(PartnerValue), partner.Value.GetType());
            var partnerValue = (PartnerValue) partner.Value;
            Assert.AreEqual(TenantId, partnerValue.TenantId);
            Assert.AreEqual(Name, partnerValue.Name);
            Assert.AreEqual(DriContact, partnerValue.ContactEmail);
            Assert.AreEqual(NotifyContact, partnerValue.NotifyContactEmail);
            var partnerCdnConfigurations = partnerValue.CdnConfigurations.ToList();
            Assert.AreEqual(1, partnerCdnConfigurations.Count);
            Assert.AreEqual(TestHostname, partnerCdnConfigurations[0].Hostname);
            var cdnWithCredentials = partnerCdnConfigurations[0].CdnCredentials;
            Assert.AreEqual(2, cdnWithCredentials.Count);
            Assert.AreEqual("", cdnWithCredentials[CDN.AFD.ToString()]);
            Assert.AreEqual("", cdnWithCredentials[CDN.Akamai.ToString()]);
        }

        [TestMethod]
        public void TestGetPartner_Success()
        {
            var testPartnerResponse = CreateTestPartner();
            Assert.AreEqual(typeof(StringResult), testPartnerResponse.GetType());
            var partnerId = ((StringResult) testPartnerResponse).Value.ToString();

            var partnerResult = partnerFunctions.GetPartner(new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    ["partnerId"] = partnerId
                })
            }, null).Result;

            Assert.AreEqual(typeof(PartnerResult), partnerResult.GetType());

            var partner = (PartnerResult) partnerResult;
            TestPartnerSerialization(partner);
        }

        [TestMethod]
        public void TestGetPartner_Fail()
        {
            var partnerResult = partnerFunctions.GetPartner(new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection()
            }, null).Result;

            Assert.AreEqual(typeof(StringResult), partnerResult.GetType());
        }

        [TestMethod]
        public void TestListPartners()
        {
            this.partners.Clear();
            CreateTestPartner();
            var partnersResponse =
                partnerFunctions.ListPartners(new DefaultHttpRequest(new DefaultHttpContext()), null).Result;
            Assert.AreEqual(typeof(EnumerableResult<PartnerResult>), partnersResponse.GetType());
            var partnersValue = ((EnumerableResult<PartnerResult>) partnersResponse).Value;

            var retrievedPartners = partnersValue as IEnumerable<PartnerResult>;
            Assert.IsNotNull(retrievedPartners);
            var retrievedPartnersList = retrievedPartners.ToList();

            Assert.AreEqual(1, retrievedPartnersList.Count);
            TestPartnerSerialization(retrievedPartnersList.First());
        }

        private IActionResult CreateTestPartner()
        {
            var createPartnerResponse = partnerFunctions.CreatePartner(new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{" +
                                                               $@"""Tenant"": ""{TenantId}""," +
                                                               $@"""Name"": ""{Name}""," +
                                                               $@"""ContactEmail"": ""{DriContact}""," +
                                                               $@"""NotifyContactEmail"": ""{NotifyContact}""," +
                                                               $@"""CdnConfiguration"": {{""Hostname"": ""{TestHostname}"", ""CdnWithCredentials"": {{""AFD"":"""", ""Akamai"":""""}}}}" +
                                                               "}"))
            }, Mock.Of<ILogger>());
            return createPartnerResponse.Result;
        }
    }
}