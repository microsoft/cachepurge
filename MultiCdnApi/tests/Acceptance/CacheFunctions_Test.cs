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
    using System.Threading.Tasks;
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CacheFunctions_Test : GenericCachePurge_Test
    {
        private CacheFunctions cacheFunctions;
        private IPartnerRequestTableManager<CDN> partnerRequestTable;
        private PartnerTable partnerTable;
        private IRequestTable<UserRequest> userRequestTable;
        private string testPartnerId;

        private const string TestDescription = "Test description";
        private const string TestTicketId = "Test ticket id";
        private const string TestHostname = "https://test.hostname.com";
        private const string TestPartnerId = "FakePartner";
        private const string TestTenantId = "FakeTenant";

        private readonly IDictionary<string, UserRequest> userRequestDict = new Dictionary<string, UserRequest>();
        private readonly IDictionary<string, AfdPartnerRequest> afdPartnerRequest = new Dictionary<string, AfdPartnerRequest>();
        private readonly IDictionary<string, AkamaiPartnerRequest> akamaiPartnerRequest = new Dictionary<string, AkamaiPartnerRequest>();
        
        [TestInitialize]
        public void Setup()
        {
            partnerTable = new PartnerTable(CdnLibraryTestHelper.MockCosmosDbContainer(new Dictionary<string, Partner>()));

            userRequestTable = new UserRequestTable(CdnLibraryTestHelper.MockCosmosDbContainer(userRequestDict));

            var afdPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(afdPartnerRequest);
            var akamaiPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(akamaiPartnerRequest);

            partnerRequestTable = new PartnerRequestTableManager(afdPartnerRequestContainer, akamaiPartnerRequestContainer);

            cacheFunctions = new CacheFunctions(partnerTable, userRequestTable, partnerRequestTable, new TelemetryConfiguration());

            const string testTenantName = TestTenantId;
            const string testPartnerName = TestPartnerId;
            const string rawCdnConfiguration = "{\"Hostname\": \"\", \"PluginIsEnabled\": {\"AFD\": true, \"Akamai\": true}}";

            var partner = new Partner(testTenantName, testPartnerName, "", new CdnConfiguration(rawCdnConfiguration));
            partnerTable.CreateItem(partner).Wait();
            testPartnerId = partner.id;
        }

        [TestMethod]
        public async Task CreateCachePurgeRequestByHostname()
        {
            var userRequestId = await CallPurgeFunctionWithDefaultParameters();
            var savedUserRequest = await userRequestTable.GetItem(userRequestId);
            var partnerRequest = await partnerRequestTable.GetPartnerRequest(savedUserRequest.id, CDN.AFD);
            AssertIsTestRequest(partnerRequest);
        }
        
        [TestMethod]
        public async Task CreateCachePurgeRequestByHostname_Fail()
        {
            var malformedCachePurgeRequest = new DefaultHttpContext().Request;
            malformedCachePurgeRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(TestHostname));
            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(
                malformedCachePurgeRequest,
                null,
                Mock.Of<ILogger>());
            Assert.IsTrue(result is JsonResult);
        }
        
        [TestMethod]
        public async Task ApiWorksWithoutPrincipals()
        {
            var malformedCachePurgeRequest = new DefaultHttpContext().Request;
            malformedCachePurgeRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(TestHostname));

            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(malformedCachePurgeRequest, null, Mock.Of<ILogger>());
            Assert.IsTrue(result is JsonResult);
            result = await cacheFunctions.CachePurgeRequestByHostnameStatus(malformedCachePurgeRequest, null, null, Mock.Of<ILogger>());
            Assert.IsTrue(result is JsonResult);
        }

        [TestMethod]
        public async Task CreateCachePurgeRequestByHostname_CosmosDbSerialization()
        {
            _ = await CallPurgeFunctionWithDefaultParameters();

            var partnerRequest = afdPartnerRequest.First();
            AssertIsTestRequest(partnerRequest.Value);
        }

        [TestMethod]
        public async Task TestCachePurgeStatus()
        {
            var userRequestId = await CallPurgeFunctionWithDefaultParameters();
            var userRequestStatusResult = await CallPurgeStatus(userRequestId);
            Assert.AreEqual(typeof(UserRequestStatusValue), userRequestStatusResult.Value.GetType());
            var userRequestStatusValue = (UserRequestStatusValue) userRequestStatusResult.Value;
            Assert.AreEqual(userRequestId, userRequestStatusValue.Id);
            Assert.AreEqual(2, userRequestStatusValue.NumTotalPartnerRequests); // we have 2 plugins
            Assert.AreEqual(0, userRequestStatusValue.NumCompletedPartnerRequests); // 0 because it is not initialized in plugins yet 
        }

        [TestMethod]
        public async Task TestCachePurgeStatus_WrongRequestId()
        {
            var statusResult = await CallPurgeStatus("MalformedRequestId");
            Assert.AreEqual(typeof(JsonResult), statusResult.GetType());
            Assert.IsNotNull(statusResult.Value, nameof(statusResult.Value) + " == null");
            Assert.IsTrue(statusResult.Value.ToString().Contains("not found"));
        }

        private static void AssertIsTestRequest(IPartnerRequest partnerRequest)
        {
            var afdPartnerRequest = partnerRequest as AfdPartnerRequest;
            Assert.IsNotNull(afdPartnerRequest);
            Assert.AreEqual($"{TestDescription} ({TestTicketId})", afdPartnerRequest.Description);
            Assert.AreEqual(1, partnerRequest.Urls.Count);
            Assert.IsTrue(partnerRequest.Urls.Contains(TestHostname));
            Assert.AreEqual(CDN.AFD.ToString(), partnerRequest.CDN);
            Assert.AreEqual(TestTenantId, afdPartnerRequest.TenantID);
            Assert.AreEqual(TestPartnerId, afdPartnerRequest.PartnerID);
            Assert.AreEqual(partnerRequest.UserRequestID, partnerRequest.UserRequestID);
        }

        private async Task<string> CallPurgeFunctionWithDefaultParameters()
        {
            var cachePurgeRequest = new DefaultHttpContext().Request;
            cachePurgeRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes("{" +
                                                                              $@"""Description"": ""{TestDescription}""," +
                                                                              $@"""TicketId"": ""{TestTicketId}""," +
                                                                              $@"""Hostname"": ""{TestHostname}""," +
                                                                              $@"""Urls"": [""{TestHostname}""]" + 
                                                                              "}"));
            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(
                cachePurgeRequest,
                testPartnerId,
                Mock.Of<ILogger>());
            Assert.AreEqual(typeof(StringResult), result.GetType());
            Assert.IsTrue(((StringResult) result).Value is string);
            return (string) ((StringResult) result).Value;
        }

        private async Task<JsonResult> CallPurgeStatus(string userRequestId)
        {
            var emptyRequest = new DefaultHttpContext().Request;
            var statusResponse = await cacheFunctions.CachePurgeRequestByHostnameStatus(
                emptyRequest,
                testPartnerId,
                userRequestId,
                Mock.Of<ILogger>());
            Assert.IsTrue(statusResponse is JsonResult);
            var userRequestStatusResult = (JsonResult) statusResponse;
            return userRequestStatusResult;
        }

        [TestCleanup]
        public void Teardown()
        {
            partnerRequestTable.Dispose();
            partnerTable.Dispose();
            userRequestTable.Dispose();
        }
    }
}