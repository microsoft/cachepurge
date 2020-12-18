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

    public class UserRequestTable : CosmosDbEntityClient, IRequestTable<UserRequest>
    {
        public UserRequestTable() : base(EnvironmentConfig.CosmosDBConnectionString, EnvironmentConfig.CosmosDatabaseId,
            EnvironmentConfig.UserRequestCosmosContainerId, EnvironmentConfig.UserRequestTablePartitionKey) {}

        public UserRequestTable(Container container) : base(container) { }

        public async Task CreateItem(UserRequest userRequest)
        {
            await Create(userRequest);
        }

        public async Task UpsertItem(UserRequest userRequest)
        {
            await Upsert(userRequest);
        }

        public async Task<UserRequest> GetItem(string id)
        {
            return await SelectFirstByIdAsync<UserRequest>(id);
        }

        public Task<IEnumerable<UserRequest>> GetItems() => throw new NotImplementedException();
    }
}