/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class AfdRequestTable : CosmosDbEntityClient, IRequestTable<AfdRequest>
    {
        public AfdRequestTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId, 
            EnvironmentConfig.AfdCdnCollectionName, EnvironmentConfig.CdnRequestTablePartitionKey) { }

        public AfdRequestTable(Container container) : base(container) { }

        public async Task CreateItem(AfdRequest cdnRequest)
        {
            await Create(cdnRequest);
        }

        public Task<AfdRequest> GetItem(string id) => throw new NotImplementedException();

        public async Task UpsertItem(AfdRequest cdnRequest)
        {
            await base.Upsert(cdnRequest);
        }

        public Task<IEnumerable<AfdRequest>> GetItems() => throw new NotImplementedException();
    }
}
