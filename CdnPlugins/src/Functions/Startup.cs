/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

using CachePurgeLibrary;
using CdnLibrary;
using CdnPlugin;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace CdnPlugin
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICdnRequestTableManager<CDN>>((s) => { return new CdnRequestTableManager(); });
            builder.Services.AddSingleton<IRequestTable<UserRequest>>((s) => { return new UserRequestTable(); });
            builder.Services.AddSingleton<IPartnerRequestTableManager<CDN>>((s) => { return new PartnerRequestTableManager(); });
        }
    }
}