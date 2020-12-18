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

    public class AfdPartnerRequestTable : CosmosDbEntityClient, IRequestTable<AfdPartnerRequest>
    {
        public AfdPartnerRequestTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId,
            EnvironmentConfig.AfdPartnerCollectionName, EnvironmentConfig.PartnerRequestTablePartitionKey) { }

        public AfdPartnerRequestTable(Container container) : base(container) { }

        public async Task CreateItem(AfdPartnerRequest partnerRequest)
        {
            await Create(partnerRequest);
        }

        public async Task<AfdPartnerRequest> GetItem(string id)
        {
            return await SelectFirstByIdAsync<AfdPartnerRequest>(id);
        }

        public async Task UpsertItem(AfdPartnerRequest partnerRequest)
        {
            await Upsert(partnerRequest);
        }

        public Task<IEnumerable<AfdPartnerRequest>> GetItems() => throw new NotImplementedException();
    }
}
