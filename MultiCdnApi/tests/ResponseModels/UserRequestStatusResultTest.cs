// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi.ResponseModels
{
    using System.Collections.Generic;
    using CachePurgeLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserRequestStatusResultTest
    {
        [TestMethod]
        public void TestSerialization()
        {
            var userRequest =
                new UserRequest("partnerId", "description", "ticketId", "hostname", new HashSet<string>()) {
                    PluginStatuses = { ["AFD"] = "Forbidden", ["Akamai"] = "Forbidden" }
                };
            var result = new UserRequestStatusResult(userRequest);
            Assert.IsInstanceOfType(result.Value, typeof(UserRequestStatusValue));
            Assert.AreEqual(((UserRequestStatusValue)result.Value).PluginStatuses["AFD"], "Forbidden");
            Assert.AreNotEqual(((UserRequestStatusValue)result.Value).PluginStatuses["Akamai"], "Forbidden");
            Assert.IsTrue(((UserRequestStatusValue)result.Value).PluginStatuses["Akamai"].StartsWith("Forbidden"));
        }
    }
}