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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CacheFunctions_Test
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

            cacheFunctions = new CacheFunctions(partnerTable, userRequestTable, partnerRequestTable);

            const string testTenantName = TestTenantId;
            const string testPartnerName = TestPartnerId;
            const string testContactEmail = "testDri@n/a.com";
            const string testNotifyContactEmail = "testNotify@n/a.com";
            const string rawCdnConfiguration = "{\"Hostname\": \"\", \"CdnWithCredentials\": {\"AFD\":\"\", \"Akamai\":\"\"}}";

            var partner = new Partner(testTenantName, testPartnerName, testContactEmail, testNotifyContactEmail, new[] { new CdnConfiguration(rawCdnConfiguration) });
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
            var defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(TestHostname))
            };
            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(
                defaultHttpRequest,
                null,
                Mock.Of<ILogger>());
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
            var defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{" +
                                                               $@"""Description"": ""{TestDescription}""," +
                                                               $@"""TicketId"": ""{TestTicketId}""," +
                                                               $@"""Hostname"": ""{TestHostname}""," +
                                                               $@"""Urls"": [""{TestHostname}""]" +
                                                               "}"))
            };
            var result = await cacheFunctions.CreateCachePurgeRequestByHostname(
                defaultHttpRequest,
                testPartnerId,
                Mock.Of<ILogger>());
            Assert.AreEqual(typeof(StringResult), result.GetType());
            Assert.IsTrue(((StringResult) result).Value is string);
            return (string) ((StringResult) result).Value;
        }
        
        private async Task<UserRequestStatusResult> CallPurgeStatus(string userRequestId)
        {
            var defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext());
            var statusResponse = await cacheFunctions.CachePurgeRequestByHostnameStatus(
                defaultHttpRequest,
                testPartnerId,
                userRequestId,
                Mock.Of<ILogger>());
            Assert.AreEqual(typeof(UserRequestStatusResult), statusResponse.GetType());
            var userRequestStatusResult = (UserRequestStatusResult) statusResponse;
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