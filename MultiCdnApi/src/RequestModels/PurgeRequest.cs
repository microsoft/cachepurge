/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;

    internal class PurgeRequest
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public IEnumerable<string> Urls { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string Description { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string TicketId { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global - used in JSON serialization/deserialization
        public string Hostname { get; set; }
    }
}