// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PartnerResult_Test
    {
        [TestMethod]
        public void TestPartnerResult_OldPartnersDontThrowException()
        {
            var cdnConfiguration = new CdnConfiguration { Hostname = "", PluginIsEnabled = null };
            var partner = new Partner("Bing", "Bing_StaticAssets", cdnConfiguration);
            var partnerResult = new PartnerResult(partner);
            Assert.AreEqual("", ((PartnerValue)partnerResult.Value).CdnConfiguration.Hostname);
            Assert.AreEqual(2, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled.Count);
            Assert.AreEqual(true, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled[CDN.Akamai.ToString()]);
            Assert.AreEqual(true, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled[CDN.AFD.ToString()]);

            partner.CdnConfiguration = null;
            partnerResult = new PartnerResult(partner);
            Assert.AreEqual("", ((PartnerValue)partnerResult.Value).CdnConfiguration.Hostname);
            Assert.AreEqual(2, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled.Count);
            Assert.AreEqual(true, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled[CDN.Akamai.ToString()]);
            Assert.AreEqual(true, ((PartnerValue)partnerResult.Value).CdnConfiguration.PluginIsEnabled[CDN.AFD.ToString()]);
        }
    }
}