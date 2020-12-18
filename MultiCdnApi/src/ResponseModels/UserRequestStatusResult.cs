/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using CachePurgeLibrary;
    using Microsoft.AspNetCore.Mvc;

    public class UserRequestStatusResult : JsonResult
    {
        public UserRequestStatusResult(UserRequest userRequest) : base(new object())
        {
            Value = new UserRequestStatusValue
            {
                Id = userRequest.id, 
                NumCompletedPartnerRequests = userRequest.NumCompletedPartnerRequests, 
                NumTotalPartnerRequests = userRequest.NumTotalPartnerRequests
            };
        }
    }
    
    public class UserRequestStatusValue
    {
        public string Id { get; set; }
        public int NumCompletedPartnerRequests { get; set; }
        public int NumTotalPartnerRequests { get; set; }
    }
}