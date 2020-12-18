/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    public abstract class CosmosDbEntity : ICosmosDbEntity
    {
        public string id { get; set; }
    }
}