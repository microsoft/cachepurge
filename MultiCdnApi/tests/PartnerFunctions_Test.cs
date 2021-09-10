// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CachePurgeLibrary;
    using CdnLibrary;
    using CdnPlugin;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PartnerFunctions_Test : GenericCachePurge_Test
    {
        private PartnerFunctions partnerFunctionsWithLocalStorage;
        
        [TestInitialize]
        public void Setup()
        {
            var partnerTable = new Mock<IRequestTable<Partner>>();
            partnerTable
                .Setup(pT => pT.GetItems())
                .Returns(Task.FromResult<IEnumerable<Partner>>(new List<Partner> {
                    new Partner("Bing", 
                        "Bing_StaticAssets", 
                        new CdnConfiguration(@"{ ""Hostname"": """", ""PluginIsEnabled"": { ""AFD"": true, ""Akamai"": true } }")),
                    new Partner("Bing", "Bing_Thumbnails", new CdnConfiguration(@"{""Hostname"": """", 
                                ""CdnWithCredentials"": { ""AFD"": """", ""Akamai"": """" } }"))
                }));
            partnerFunctionsWithLocalStorage = new PartnerFunctions(partnerTable.Object);
        }

        [TestMethod]
        public void TestPartnerFunctions_WorksWithOldFormats()
        {
            var response 
                = partnerFunctionsWithLocalStorage.ListPartners(Mock.Of<HttpRequest>(), Mock.Of<ILogger>()).Result;
            Assert.AreEqual(typeof(JsonResult), response.GetType());
            var partners = (JsonResult) response;
            Assert.AreEqual(typeof(Partner[]), partners.Value.GetType());
            var partnerResults = (Partner[]) partners.Value;
            Assert.AreEqual(2, partnerResults.Length);
            Assert.AreEqual(typeof(Partner), partnerResults[0].GetType());
            Assert.AreEqual(typeof(Partner), partnerResults[1].GetType());

            AssertIsValidPartner(partnerResults[0], "Bing", "Bing_StaticAssets");
        }

        private static void AssertIsValidPartner(Partner partnerValue, string tenantId, string name, string hostname = "")
        {
            Assert.AreEqual(tenantId, partnerValue.TenantId);
            Assert.AreEqual(name, partnerValue.Name);
            Assert.AreEqual(hostname, partnerValue.CdnConfiguration.Hostname);
            Assert.AreEqual(2, partnerValue.CdnConfiguration.PluginIsEnabled.Count);
            Assert.AreEqual(true, partnerValue.CdnConfiguration.PluginIsEnabled[CDN.Akamai.ToString()]);
            Assert.AreEqual(true, partnerValue.CdnConfiguration.PluginIsEnabled[CDN.AFD.ToString()]);
        }
    }
}