// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using CdnPlugin;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class GenericCachePurge_Test
    {
        protected GenericCachePurge_Test()
        {
            TestEnvironmentConfig.SetupTestEnvironment();
        }
    }
}