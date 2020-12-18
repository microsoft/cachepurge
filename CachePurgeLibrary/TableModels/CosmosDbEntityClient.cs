/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;

    public abstract class CosmosDbEntityClient: IDisposable
    {
        private readonly string cosmosDbId;
        private readonly string containerId;
        private readonly string partitionKey = "/id";

        private readonly CosmosClient cosmosClient;
        protected Container Container;

        protected CosmosDbEntityClient(string connectionString, string databaseId, string containerId)
        {
            this.containerId = containerId;
            this.cosmosDbId = databaseId;
            CosmosClientOptions clientOptions = new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    IgnoreNullValues = true
                }
            };

            cosmosClient = new CosmosClient(connectionString, clientOptions);
        }

        protected CosmosDbEntityClient(Container container)
        {
            Container = container;
        }

        protected CosmosDbEntityClient(string connectionString, string databaseId, string containerId, string partitionKey) : this(connectionString, databaseId, containerId)
        {
            this.partitionKey = $"/{partitionKey}";
        }

        protected async Task Create<T>(T item)
        {
            if (Container == null)
            {
                await CreateContainer();
            }

            await Container.CreateItemAsync(item);
        }

        protected async Task Upsert<T>(T item)
        {
            if (Container == null)
            {
                await CreateContainer();
            }

            await Container.UpsertItemAsync(item);
        }

        protected async Task<T> SelectFirstByIdAsync<T>(string id, string indexColumnName = "id")
        {
            if (Container == null)
            {
                await CreateContainer();
            }

            using var queryIterator = Container.GetItemQueryIterator<T>(
                $"SELECT * FROM {containerId} c WHERE c.{indexColumnName} = '{id}'");
            if (queryIterator != null && queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                if (response.Count > 0)
                {
                    return response.First();
                }
            }
            return default;
        }

        protected async Task CreateContainer()
        {
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDbId);
            Container = await database.Database.CreateContainerIfNotExistsAsync(containerId, partitionKey);
        }

        public void Dispose()
        {
            if (cosmosClient != null)
            {
                cosmosClient.Dispose();
            }
        }
    }
}