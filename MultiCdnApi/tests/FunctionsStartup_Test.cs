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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    public class FunctionsStartup_Test
    {
        private IFunctionsHostBuilder functionsHostBuilder;

        [TestInitialize]
        public void Setup()
        {
            var multiCdnFunctionsStartup = new Startup();
            functionsHostBuilder = Mock.Of<IFunctionsHostBuilder>(b => b.Services == new ServiceCollection());
            multiCdnFunctionsStartup.Configure(functionsHostBuilder);
        }

        [TestMethod]
        public void TestSetupPartnerTable()
        {
            var partnerTableService = FindServiceByType(functionsHostBuilder, typeof(IRequestTable<Partner>));
            Assert.IsNotNull(partnerTableService);
        }

        [TestMethod]
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