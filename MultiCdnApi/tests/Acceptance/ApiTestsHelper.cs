// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Security.Claims;
    using System.Security.Principal;

    public static class ApiTestsHelper
    {
        internal static ClaimsPrincipal CreateTestClaimsPrincipal()
        {
            return new GenericPrincipal(new GenericIdentity("test-user"), new []{"test-role"});
        }
    }
}