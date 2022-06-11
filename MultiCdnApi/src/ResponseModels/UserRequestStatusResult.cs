/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using Azure.Core;
    using CachePurgeLibrary;
    using CdnLibrary;
    using Microsoft.AspNetCore.Mvc;

    public class UserRequestStatusResult : JsonResult
    {
        public UserRequestStatusResult(UserRequest userRequest) : base(new object())
        {
            var userRequestPluginStatuses = userRequest.PluginStatuses;
            if (userRequestPluginStatuses.ContainsKey(CDN.Akamai.ToString()))
            {
                var status = userRequestPluginStatuses[CDN.Akamai.ToString()];
                if (status == RequestStatus.Forbidden.ToString())
                {
                    userRequestPluginStatuses[CDN.Akamai.ToString()] = "Forbidden; either the partner wasn't setup in Akamai or CachePurge doesn't have permissions to purge its caches (the first option happens more often than the second)";
                }
            }
            Value = new UserRequestStatusValue
            {
                Id = userRequest.id, 
                NumCompletedPartnerRequests = userRequest.NumCompletedPartnerRequests, 
                NumTotalPartnerRequests = userRequest.NumTotalPartnerRequests,
                PluginStatuses = userRequestPluginStatuses
            };
        }
    }
    
    public class UserRequestStatusValue
    {
        public string Id { get; set; }
        public int NumCompletedPartnerRequests { get; set; }
        public int NumTotalPartnerRequests { get; set; }
        public IDictionary<string, string> PluginStatuses { get; set; }
    }
}