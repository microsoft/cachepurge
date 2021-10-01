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

            testPartnerId = CreatePartner();
        }

        private string CreatePartner(bool enableAfd = true, bool enableAkamai = true)
        {
            var partner = new Partner(TestTenantId, TestPartnerId, "", new CdnConfiguration(
                $@"{{
                    ""Hostname"": """", 
                    ""PluginIsEnabled"": {{
                        ""AFD"": {enableAfd.ToString().ToLower()}, 
                        ""Akamai"": {enableAkamai.ToString().ToLower()}
                     }}
                }}"));
            partnerTable.CreateItem(partner).Wait();
            return partner.id;
        }

        [TestMethod]
        public async Task CreateCachePurgeRequestByHostname()
        {
            var userRequestId = await CallPurgeFunctionWithDefaultParameters(testPartnerId);
            var savedUserRequest = await userRequestTable.GetItem(userRequestId);
            var partnerRequest = await partnerRequestTable.GetPartnerRequest(savedUserRequest.id, CDN.AFD);
            AssertIsAfdTestRequest(partnerRequest);
        }
        
        [TestMethod]
        public async Task CreateCachePurgeRequestByHostname_WithTags()
        {
            const string testTag = "testTag";
            var savedUserRequest = await CallPurgeFunctionWithDefaultParameters(testPartnerId, urls: testTag, treatUrlsLikeTags: true);
            var partnerRequestForAfd = await partnerRequestTable.GetPartnerRequest(savedUserRequest, CDN.AFD);
            AssertIsAfdTestRequest(partnerRequestForAfd, testTag);
            var partnerRequestForAkamai = await partnerRequestTable.GetPartnerRequest(savedUserRequest, CDN.Akamai);
            AssertIsAkamaiTestRequest(partnerRequestForAkamai, testTag);
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
            _ = await CallPurgeFunctionWithDefaultParameters(testPartnerId);

            var partnerRequest = afdPartnerRequest.First();
            AssertIsAfdTestRequest(partnerRequest.Value);
        }

        [TestMethod]
        public async Task TestRelativeUrls()
        {
            const string relativeUrl = "test";
            var savedUserRequest = await CallPurgeFunctionWithDefaultParameters(testPartnerId, urls: relativeUrl);
            var partnerRequestForAfd = await partnerRequestTable.GetPartnerRequest(savedUserRequest, CDN.AFD);
            AssertIsAfdTestRequest(partnerRequestForAfd, TestHostname + "/" + relativeUrl);
        }

        [TestMethod]
        public async Task TestRelativeUrls_FailWithoutBaseHostname()
        {
            Assert.ThrowsExceptionAsync<InvalidOperationException>(() => CallPurgeFunctionWithDefaultParameters("", "test"));
        }

        [TestMethod]
        public async Task TestCachePurgeStatus()
        {
            var userRequestId = await CallPurgeFunctionWithDefaultParameters(testPartnerId);
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

        private static void AssertIsAfdTestRequest(IPartnerRequest partnerRequest, string urlToPurge = TestHostname)
        {
            AssertIsTestRequest(partnerRequest, CDN.AFD, urlToPurge);
            var afdPartnerRequest = partnerRequest as AfdPartnerRequest;
            Assert.IsNotNull(afdPartnerRequest);
            Assert.AreEqual($"{TestDescription} ({TestTicketId})", afdPartnerRequest.Description);
            Assert.AreEqual(TestTenantId, afdPartnerRequest.TenantID);
            Assert.AreEqual(TestPartnerId, afdPartnerRequest.PartnerID);
        }

        private static void AssertIsAkamaiTestRequest(IPartnerRequest partnerRequest, string urlToPurge = TestHostname)
        {
            AssertIsTestRequest(partnerRequest, CDN.Akamai, urlToPurge);
            var akamaiPartnerRequest = partnerRequest as AkamaiPartnerRequest;
            Assert.IsNotNull(akamaiPartnerRequest);
            Assert.AreEqual(null, akamaiPartnerRequest.Network);
        }

        private static void AssertIsTestRequest(IPartnerRequest partnerRequest, CDN cdn, string urlToPurge = TestHostname)
        {
            Assert.AreEqual(1, partnerRequest.Urls.Count);
            Assert.IsTrue(partnerRequest.Urls.Contains(urlToPurge));
            Assert.AreEqual(cdn.ToString(), partnerRequest.CDN);
            Assert.AreEqual(partnerRequest.UserRequestID, partnerRequest.UserRequestID);
        }

        private async Task<string> CallPurgeFunctionWithDefaultParameters(string partnerId, string hostname = TestHostname, string urls = TestHostname, bool treatUrlsLikeTags = false)
        {
            var cachePurgeRequest = new DefaultHttpContext().Request;
            cachePurgeRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes("{" +
                                                                              $@"""Description"": ""{TestDescription}""," +
                                                                              $@"""TicketId"": ""{TestTicketId}""," +
                                                                              $@"""Hostname"": ""{hostname}""," +
                                                                              $@"""Urls"": [""{urls}""]," + 
                                                                              $@"""TreatUrlsLikeTags"": {treatUrlsLikeTags.ToString().ToLower()}" + 
                                                                              "}"));
            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(
                cachePurgeRequest,
                partnerId,
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