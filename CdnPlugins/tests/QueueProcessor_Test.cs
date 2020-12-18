/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnPlugin
{
    using CdnLibrary;
    using CdnLibrary_Test;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    [TestClass]
    public class QueueProcessor_Test
    {
        private readonly IDictionary<string, AfdRequest> afdRequest = new Dictionary<string, AfdRequest>();
        private readonly IDictionary<string, AkamaiRequest> akamaiRequest = new Dictionary<string, AkamaiRequest>();

        private readonly List<CloudQueueMessage> OutputQueue = new List<CloudQueueMessage>();

        private static readonly string[] urls = new string[] { "https://fakeUrls" };

        private QueueProcessorFunctions queueFunctions;

        [TestInitialize]
        public void Setup()
        {
            var afdRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(afdRequest);
            var akamaiRequestContainer = CdnLibraryTestHelper.MockCosmosDbContainer(akamaiRequest);

            var cdnRequestTableManager = new CdnRequestTableManager(afdRequestContainer, akamaiRequestContainer);

            queueFunctions = new QueueProcessorFunctions(cdnRequestTableManager);
        }

        [TestMethod]
        public void ProcessAfd_EmptyRequest()
        {
            var request = new AfdRequest();

            var cloudQueueMsg=  new CloudQueueMessage(JsonSerializer.Serialize(request));

            queueFunctions.ProcessAfd(cloudQueueMsg, CreateCollector(), Mock.Of<ILogger>()).Wait();

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessAfd_Success()
        {
            var afdRequestBody = new AfdRequestBody()
            {
                Urls = urls,
                Description = "Test"
            };

            var request = new AfdRequest()
            {
                id = "1",
                Urls = urls,
                RequestBody = JsonSerializer.Serialize(afdRequestBody, CdnPluginHelper.JsonSerializerOptions),
                Endpoint = "testendpoint"
            };

            var cloudQueueMsg = new CloudQueueMessage(JsonSerializer.Serialize(request));

            queueFunctions.ProcessAfd(cloudQueueMsg, CreateCollector(), Mock.Of<ILogger>()).Wait();

            Assert.AreEqual(1, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessAkamai_Fail()
        {
            var request = new AkamaiRequest();

            var cloudQueueMsg = new CloudQueueMessage(JsonSerializer.Serialize(request));

            queueFunctions.ProcessAkamai(cloudQueueMsg, CreateCollector(), Mock.Of<ILogger>()).Wait();

            Assert.AreEqual(0, OutputQueue.Count);
        }

        [TestMethod]
        public void ProcessAkamai_Success()
        {
            var afdRequestBody = new AkamaiRequestBody()
            {
                Hostname = "testhostname",
                Objects=urls
            };

            var request = new AkamaiRequest()
            {
                id = "1",
                Urls = urls,
                RequestBody = JsonSerializer.Serialize(afdRequestBody, CdnPluginHelper.JsonSerializerOptions),
                Endpoint = "testendpoint"
            };

            var cloudQueueMsg = new CloudQueueMessage(JsonSerializer.Serialize(request));

            queueFunctions.ProcessAfd(cloudQueueMsg, CreateCollector(), Mock.Of<ILogger>()).Wait();

            Assert.AreEqual(1, OutputQueue.Count);
        }

        private CloudQueue CreateCollector()
        {
            var collector = new Mock<TestCloudQueue>();

            collector.Setup(c => c.AddMessage(
                It.IsAny<CloudQueueMessage>(), 
                It.IsAny<TimeSpan?>(), 
                It.IsAny<TimeSpan?>(), 
                It.IsAny<QueueRequestOptions>(), 
                It.IsAny<OperationContext>())).Callback<CloudQueueMessage, TimeSpan?, TimeSpan?, QueueRequestOptions, OperationContext>((s, a, b, c, d) => OutputQueue.Add(s));

            return collector.Object;
        }
    }

    public class TestCloudQueue : CloudQueue
    {
        public TestCloudQueue() : base(new Uri("http://fakeUri")) { }
    } 
}
