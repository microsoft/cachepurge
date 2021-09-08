// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System;
    using Azure.Identity;
    using CdnLibrary;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserGroupAuthValidator_Test : GenericCachePurge_Test
    {
        private string authorizationEnabledPreviousValue;

        [TestInitialize]
        public void Setup()
        {
            authorizationEnabledPreviousValue = Environment.GetEnvironmentVariable(EnvironmentConfig.AuthorizationEnabledEnvironmentVariable);
            Environment.SetEnvironmentVariable(EnvironmentConfig.AuthorizationEnabledEnvironmentVariable, "True");
        }
        
        [TestMethod]
        public void TestAuth_ThrowsExceptionWhenNotAuthorized()
        {
            var httpRequest = new DefaultHttpContext().Request;
            Assert.ThrowsException<AuthenticationFailedException>(
                () => UserGroupAuthValidator.CheckUserAuthorized(httpRequest));
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(EnvironmentConfig.AuthorizationEnabledEnvironmentVariable, authorizationEnabledPreviousValue);
        }
    }
}