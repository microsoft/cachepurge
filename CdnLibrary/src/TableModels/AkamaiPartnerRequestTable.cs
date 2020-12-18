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

    public class AkamaiPartnerRequestTable : CosmosDbEntityClient, IRequestTable<AkamaiPartnerRequest>
    {
        public AkamaiPartnerRequestTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId, 
            EnvironmentConfig.AkamaiPartnerCollectionName, EnvironmentConfig.PartnerRequestTablePartitionKey) { }

        public AkamaiPartnerRequestTable(Container container) : base(container) { }

        public async Task CreateItem(AkamaiPartnerRequest partnerRequest)
        {
            await Create(partnerRequest);
        }

        public async Task<AkamaiPartnerRequest> GetItem(string id)
        {
            return await SelectFirstByIdAsync<AkamaiPartnerRequest>(id);
        }

        public async Task UpsertItem(AkamaiPartnerRequest partnerRequest)
        {
            await Upsert(partnerRequest);
        }

        public Task<IEnumerable<AkamaiPartnerRequest>> GetItems() => throw new NotImplementedException();
    }
}
