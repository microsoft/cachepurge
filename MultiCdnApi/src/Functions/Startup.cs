/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MultiCdnApi;
using MultiCdnApi.Swagger;
using CachePurgeLibrary;
using CdnLibrary;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MultiCdnApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IRequestTable<Partner>>(s => new PartnerTable());
            builder.Services.AddSingleton<IRequestTable<UserRequest>>(s => new UserRequestTable());
            builder.Services.AddSingleton<IPartnerRequestTableManager<CDN>>(s => new PartnerRequestTableManager());

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(),
                options => {
                    options.ConfigureSwaggerGen = genOptions => genOptions.OperationFilter<PostContentFilter>();
                });
        }
    }
}