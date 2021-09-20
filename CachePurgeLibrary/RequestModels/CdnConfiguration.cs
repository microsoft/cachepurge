/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using System.Collections.Generic;
    using System.Text.Json;

    public class CdnConfiguration
    {
        public IDictionary<string, bool> PluginIsEnabled { get; set; }

        public CdnConfiguration()
        {
            PluginIsEnabled = new Dictionary<string, bool>();
        }
        
        public CdnConfiguration(string rawCdnConfiguration)
        {
            var cdnConfiguration = JsonSerializer.Deserialize<CdnConfiguration>(rawCdnConfiguration);
            PluginIsEnabled = cdnConfiguration.PluginIsEnabled;
        }

        public override string ToString()
        {
            return $"{nameof(PluginIsEnabled)}: {PluginIsEnabled}";
        }
    }
}