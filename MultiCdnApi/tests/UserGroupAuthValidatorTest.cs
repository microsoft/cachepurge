// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using Azure.Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserGroupAuthValidatorTest
    {
        [TestMethod]
        public void TestAuth_ThrowsExceptionWhenNotAuthorized()
        {
            var httpRequest = new DefaultHttpContext().Request;
            Assert.ThrowsException<AuthenticationFailedException>(
                () => UserGroupAuthValidator.CheckUserAuthorized(httpRequest));
        }
    }
}