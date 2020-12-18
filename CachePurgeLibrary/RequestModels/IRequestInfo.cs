/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    internal interface IRequestInfo
    {
        public string RequestID { get; set; }

        public RequestStatus RequestStatus { get; set; }
    }

    public enum RequestStatus
    {
        Error,
        Unauthorized,
        Throttled,
        Unknown,
        BatchCreated,
        MaxRetry,
        PurgeSubmitted,
        PurgeCompleted,
    };
}
