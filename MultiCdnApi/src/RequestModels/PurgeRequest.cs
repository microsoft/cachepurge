/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;

    internal class PurgeRequest
    {
        public IEnumerable<string> Urls { get; set; }

        public string Description { get; set; }

        public string TicketId { get; set; }

        public string Hostname { get; set; }
    }
}