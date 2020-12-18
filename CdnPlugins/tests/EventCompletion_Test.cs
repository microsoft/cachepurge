/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnPlugin
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;

    [TestClass]
    public class EventCompletion_Test
    {
        private readonly ILogger logger = Mock.Of<ILogger>();
        private static readonly string PurgeCompleted = RequestStatus.PurgeCompleted.ToString();

        private readonly IDictionary<string, AfdPartnerRequest> afdPartnerRequest = new Dictionary<string, AfdPartnerRequest>();
        private readonly IDictionary<string, AkamaiPartnerRequest> akamaiPartnerRequest = new Dictionary<string, AkamaiPartnerRequest>();

        EventCompletionFunctions completionFunctions;

        [TestInitialize]
        public void Setup()
        {
            var afdPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(afdPartnerRequest);
            var akamaiPartnerRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(akamaiPartnerRequest);

            var partnerRequestTable = new PartnerRequestTableManager(afdPartnerRequestContainer, akamaiPartnerRequestContainer);
            completionFunctions = new EventCompletionFunctions(null, partnerRequestTable);
        }

        [TestMethod]
        public void UpdatePartnerRequest_Success()
        {
            var partnerRequest = new PartnerRequest()
            {
                NumCompletedCdnRequests = 0,
                NumTotalCdnRequests = 1,
                Status = "BatchCreated"
            };

            Assert.IsTrue(completionFunctions.UpdatePartnerRequest(partnerRequest, PurgeCompleted, logger));
            Assert.AreEqual(1, partnerRequest.NumCompletedCdnRequests);
            Assert.AreEqual(PurgeCompleted, partnerRequest.Status);
        }

        [TestMethod]
        public void UpdatePartnerRequest_ErrorNoStatus()
        {
            var partnerRequest = new PartnerRequest()
            {
                NumCompletedCdnRequests = 0,
                NumTotalCdnRequests = 1
            };

            Assert.IsFalse(completionFunctions.UpdatePartnerRequest(partnerRequest, PurgeCompleted, logger));
        }

        [TestMethod]
        public void UpdatePartnerRequest_ErrorStatusPreserved()
        {
            var partnerRequest = new PartnerRequest()
            {
                NumCompletedCdnRequests = 0,
                NumTotalCdnRequests = 1,
                Status = "Error"
            };

            Assert.IsTrue(completionFunctions.UpdatePartnerRequest(partnerRequest, "Error", logger));
            Assert.AreEqual(1, partnerRequest.NumCompletedCdnRequests);
            Assert.AreEqual("Error", partnerRequest.Status);
        }

        [TestMethod]
        public void UpdatePartnerRequest_NotComplete()
        {
            var partnerRequest = new PartnerRequest()
            {
                NumCompletedCdnRequests = 0,
                NumTotalCdnRequests = 2,
                Status = "BatchCreated"
            };

            Assert.IsTrue(completionFunctions.UpdatePartnerRequest(partnerRequest, PurgeCompleted, logger));
            Assert.AreEqual(1, partnerRequest.NumCompletedCdnRequests);
            Assert.AreEqual("BatchCreated", partnerRequest.Status);
        }

        [TestMethod]
        public void UpdatePartnerRequest_CdnRequestError()
        {
            var partnerRequest = new PartnerRequest()
            {
                NumCompletedCdnRequests = 0,
                NumTotalCdnRequests = 2,
                Status = "Error"
            };

            Assert.IsTrue(completionFunctions.UpdatePartnerRequest(partnerRequest, "Error", logger));
            Assert.AreEqual(1, partnerRequest.NumCompletedCdnRequests);
            Assert.AreEqual("Error", partnerRequest.Status);
        }
    }
}
