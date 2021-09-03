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
        public string Hostname { get; set; }
        
        public IDictionary<string, bool> PluginIsEnabled { get; set; }

        // Credentials are not used for now anyway
        // public IDictionary<string, string> CdnWithCredentials { get; set; }

        public CdnConfiguration()
        {
            // CdnWithCredentials = new Dictionary<string, string>();
            PluginIsEnabled = new Dictionary<string, bool>();
        }
        
        public CdnConfiguration(string rawCdnConfiguration)
        {
            var cdnConfiguration = JsonSerializer.Deserialize<CdnConfiguration>(rawCdnConfiguration);
            Hostname = cdnConfiguration.Hostname;
            // CdnWithCredentials = cdnConfiguration.CdnWithCredentials;
            PluginIsEnabled = cdnConfiguration.PluginIsEnabled;
        }

        public override string ToString()
        {
            return $"{nameof(Hostname)}: {Hostname}, "; // {nameof(CdnWithCredentials)}: <skipping credentials...>";
        }
    }
}