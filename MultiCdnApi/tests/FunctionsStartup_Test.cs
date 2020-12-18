/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System;
    using System.Linq;
    using CachePurgeLibrary;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class FunctionsStartup_Test
    {
        private IFunctionsHostBuilder functionsHostBuilder;

        [SetUp]
        public void Setup()
        {
            var multiCdnFunctionsStartup = new Startup();
            functionsHostBuilder = Mock.Of<IFunctionsHostBuilder>(b => b.Services == new ServiceCollection());
            multiCdnFunctionsStartup.Configure(functionsHostBuilder);
        }

        [Test]
        public void TestSetupPartnerTable()
        {
            var partnerTableService = FindServiceByType(functionsHostBuilder, typeof(IRequestTable<Partner>));
            Assert.IsNotNull(partnerTableService);
        }

        [Test]
        public void TestSetupUserRequestTable()
        {
            var userRequestTable = FindServiceByType(functionsHostBuilder, typeof(IRequestTable<UserRequest>));
            Assert.IsNotNull(userRequestTable);
        }

        private static ServiceDescriptor FindServiceByType(IFunctionsHostBuilder functionsHostBuilder, Type type)
        {
            return functionsHostBuilder.Services.First(t => t.ServiceType == type);
        }
    }
}