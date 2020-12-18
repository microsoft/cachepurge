/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRequestTable<T>: IDisposable
    {
        public Task CreateItem(T request);

        public Task UpsertItem(T request);

        public Task<T> GetItem(string id);

        public Task<IEnumerable<T>> GetItems();
    }
}