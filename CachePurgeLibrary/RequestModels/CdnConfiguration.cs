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

        public IDictionary<string, string> CdnWithCredentials { get; set; }

        public CdnConfiguration()
        {
            CdnWithCredentials = new Dictionary<string, string>();
        }
        
        public CdnConfiguration(string rawCdnConfiguration)
        {
            var cdnConfiguration = JsonSerializer.Deserialize<CdnConfiguration>(rawCdnConfiguration);
            Hostname = cdnConfiguration.Hostname;
            CdnWithCredentials = cdnConfiguration.CdnWithCredentials;
        }

        public override string ToString()
        {
            return $"{nameof(Hostname)}: {Hostname}, {nameof(CdnWithCredentials)}: <skipping credentials...>";
        }
    }
}