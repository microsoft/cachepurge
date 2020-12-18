/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MultiCdnApi;
using CachePurgeLibrary;
using CdnLibrary;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MultiCdnApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IRequestTable<Partner>>((s) => { return new PartnerTable(); });
            builder.Services.AddSingleton<IRequestTable<UserRequest>>((s) => { return new UserRequestTable(); });
            builder.Services.AddSingleton<IPartnerRequestTableManager<CDN>>((s) => { return new PartnerRequestTableManager(); });
        }
    }
}