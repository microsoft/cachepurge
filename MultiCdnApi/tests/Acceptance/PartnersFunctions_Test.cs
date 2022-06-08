/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PartnersFunctions_Test : GenericCachePurge_Test
    {
        private PartnerFunctions partnerFunctions;
        private IRequestTable<Partner> partnerTable;

        private const string TenantId = "FakeTenant";
        private const string Name = "FakePartner";
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
            var malformedCreatePartnerRequest = new DefaultHttpContext().Request;
            malformedCreatePartnerRequest.Query = new QueryCollection(new Dictionary<string, StringValues> {["Test"] = "Bad"});
            var createPartnerResponse = partnerFunctions.CreatePartner(malformedCreatePartnerRequest, 
                Mock.Of<ILogger>()).Result;

            Assert.AreEqual(typeof(ExceptionResult), createPartnerResponse.GetType());
        }

        private static void TestPartner(Partner partner)
        {
            Assert.AreEqual(TenantId, partner.TenantId);
            Assert.AreEqual(Name, partner.Name);
            Assert.AreEqual(TestHostname, partner.Hostname);
            var partnerCdnConfiguration = partner.CdnConfiguration;
            Assert.IsTrue(partnerCdnConfiguration.PluginIsEnabled[CDN.AFD.ToString()]);
            Assert.IsTrue(partnerCdnConfiguration.PluginIsEnabled[CDN.Akamai.ToString()]);
        }


        
        private static void TestPartnerSerialization(JsonResult partner)
        {
            Partner partnerValue;
            if (partner.Value.GetType() == typeof(Partner[]))
            {
                Assert.AreEqual(1, ((Partner[])partner.Value).Length);
                partnerValue = ((Partner[]) partner.Value)[0];
            }
            else
            {
                partnerValue = (Partner)partner.Value;
            }
            Assert.AreEqual(TenantId, partnerValue.TenantId);
            Assert.AreEqual(Name, partnerValue.Name);
            Assert.AreEqual(TestHostname, partnerValue.Hostname);
            var partnerCdnConfiguration = partnerValue.CdnConfiguration;
            Assert.IsTrue(partnerCdnConfiguration.PluginIsEnabled[CDN.AFD.ToString()]);
            Assert.IsTrue(partnerCdnConfiguration.PluginIsEnabled[CDN.Akamai.ToString()]);
        }

        [TestMethod]
        public void TestGetPartner_Success()
        {
            var testPartnerResponse = CreateTestPartner();
            Assert.AreEqual(typeof(StringResult), testPartnerResponse.GetType());
            var partnerId = ((StringResult) testPartnerResponse).Value.ToString();

            var getPartnerRequest = new DefaultHttpContext().Request;
            getPartnerRequest.Query = new QueryCollection(new Dictionary<string, StringValues> {["partnerId"] = partnerId});

            var partnerResult = partnerFunctions.GetPartner(getPartnerRequest, Guid.Parse(partnerId), 
                Mock.Of<ILogger>()).Result;

            Assert.AreEqual(typeof(JsonResult), partnerResult.GetType());

            var partner = (JsonResult) partnerResult;
            TestPartnerSerialization(partner);
        }

        [TestMethod]
        public void TestGetPartner_Fail()
        {
            var partnerResult = partnerFunctions.GetPartner(new DefaultHttpContext().Request, Guid.Empty, 
                Mock.Of<ILogger>()).Result;

            Assert.AreEqual(typeof(JsonResult), partnerResult.GetType());
            Assert.IsTrue(((JsonResult)partnerResult).Value.ToString().Contains("not found"));
        }

        [TestMethod]
        public void TestListPartners()
        {
            this.partners.Clear();
            CreateTestPartner();
            var partnersResponse =
                partnerFunctions.ListPartners(new DefaultHttpContext().Request, 
                    Mock.Of<ILogger>()).Result;
            Assert.AreEqual(typeof(JsonResult), partnersResponse.GetType());
            var partnersValue = ((JsonResult) partnersResponse).Value;

            var retrievedPartners = partnersValue as Partner[];
            Assert.IsNotNull(retrievedPartners);
            var retrievedPartnersList = retrievedPartners.ToList();

            Assert.AreEqual(1, retrievedPartnersList.Count);
            TestPartnerSerialization((JsonResult)partnersResponse);
        }

        private IActionResult CreateTestPartner()
        {
            var createPartnerRequest = new DefaultHttpContext().Request;
            createPartnerRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes("{" +
                                                                              $@"""Tenant"": ""{TenantId}""," +
                                                                              $@"""Name"": ""{Name}""," +
                                                                              $@"""Hostname"": ""{TestHostname}""," +
                                                                              $@"""CdnConfiguration"": {{""PluginIsEnabled"": {{""AFD"": true, ""Akamai"": true}}}}" +
                                                                              "}"));

            var createPartnerResponse = partnerFunctions.CreatePartner(createPartnerRequest, 
                Mock.Of<ILogger>());
            return createPartnerResponse.Result;
        }
        
        [TestCleanup]
        public void Teardown()
        {
            partnerTable.Dispose();
        }
    }
}