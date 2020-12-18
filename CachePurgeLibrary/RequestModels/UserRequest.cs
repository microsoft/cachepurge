/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Collections.Generic;

    public class UserRequest : CosmosDbEntity
    {
        public UserRequest(string partnerId, string description, string ticketId, string hostname, ISet<string> urls)
        {
            PartnerId = partnerId;
            Description = description;
            TicketId = ticketId;
            Hostname = hostname;
            Urls = urls;
            id = Guid.NewGuid().ToString();
        }

        public string PartnerId { get; }

        public string Description { get; }

        /// <summary>
        /// TicketId is an id of an item in a task-tracking system
        /// </summary>
        public string TicketId { get; }

        public string Hostname { get; }

        public ISet<string> Urls { get; }

        public int NumTotalPartnerRequests { get; set; }

        public int NumCompletedPartnerRequests { get; set; }
    }
}