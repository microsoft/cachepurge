/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.Azure.Cosmos;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class PartnerTable : CosmosDbEntityClient, IRequestTable<Partner>
    {
        private static readonly string ContainerId = EnvironmentConfig.PartnerCosmosContainerId;

        public PartnerTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId,
            EnvironmentConfig.PartnerCosmosContainerId, "id") { }

        public PartnerTable(Container container) : base(container) { }

        public async Task<IEnumerable<Partner>> GetItems()
        {
            if (Container == null)
            {
                await CreateContainer();
            }

            using var queryIterator = Container.GetItemQueryIterator<Partner>(
                $"SELECT * FROM {ContainerId} c");
            var partners = new List<Partner>();
            while (queryIterator.HasMoreResults)
            {
                var feedResponse = await queryIterator.ReadNextAsync();
                foreach (var partner in feedResponse)
                {
                    partners.Add(partner);
                }
            }
            return partners;
        }

        public async Task CreateItem(Partner request)
        {
            await Create(request);
        }

        public async Task UpsertItem(Partner request)
        {
            await Upsert(request);
        }

        public async Task<Partner> GetItem(string id)
        {
            return await SelectFirstByIdAsync<Partner>(id);
        }
    }
}