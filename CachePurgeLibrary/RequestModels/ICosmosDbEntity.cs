/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System.Diagnostics.CodeAnalysis;

    public interface ICosmosDbEntity
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Needs to be lowercase for CosmosDB to use this value as the id of the item")]

        public string id { get; set; }
    }
}