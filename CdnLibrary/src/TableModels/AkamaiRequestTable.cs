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

    internal class AkamaiRequestTable : CosmosDbEntityClient, IRequestTable<AkamaiRequest>
    {
        public AkamaiRequestTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId, 
            EnvironmentConfig.AkamaiCdnCollectionName, EnvironmentConfig.CdnRequestTablePartitionKey) { }

        public AkamaiRequestTable(Container container) : base(container) { }

        public async Task CreateItem(AkamaiRequest cdnRequest)
        {
            await Create(cdnRequest);
        }

        public Task<AkamaiRequest> GetItem(string id) => throw new NotImplementedException();


        public async Task UpsertItem(AkamaiRequest cdnRequest)
        {
            await Upsert(cdnRequest);
        }

        public Task<IEnumerable<AkamaiRequest>> GetItems() => throw new NotImplementedException();
    }
}
