/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System;
    using System.Collections.Generic;

    public class Partner : CosmosDbEntity
    {
        public string TenantId { 
            get;
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - used to deserialize Cosmos DB objects
            set; 
        } // todo: sync: do plugins really need this too?
        public string Name { 
            get; 
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - used to deserialize Cosmos DB objects
            set; 
        }
        public string ContactEmail { 
            get;
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - used to deserialize Cosmos DB objects
            set; 
        }

        public string NotifyContactEmail
        {
            get; 
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - used to deserialize Cosmos DB objects
            set;
        }
        public IEnumerable<CdnConfiguration> CdnConfigurations { 
            get; 
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global, MemberCanBePrivate.Global - used to deserialize Cosmos DB objects
            set; 
        }

        public Partner(string tenant, string name, string contactEmail, string notifyContactEmail,
            CdnConfiguration[] cdnConfigurations)
        {
            id = Guid.NewGuid().ToString();
            Name = name;
            TenantId = tenant;
            ContactEmail = contactEmail;
            NotifyContactEmail = notifyContactEmail;
            CdnConfigurations = cdnConfigurations;
        }
    }
}