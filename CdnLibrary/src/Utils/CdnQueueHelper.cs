/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System;

    internal static class CdnQueueHelper
    {
        public readonly static string Throttled = RequestStatus.Throttled.ToString();
        public readonly static string Error = RequestStatus.Error.ToString();

        public static bool AddCdnRequestToDB(ICdnRequest queueMsg, int maxRetry)
        {
            // If the request is throttled or has a generic error, it's retried until max retry
            // Only update the DB once its exhausted retries to set the final state correctly while minimizing updates
            if ((queueMsg.Status.Equals(Throttled, StringComparison.OrdinalIgnoreCase) || queueMsg.Status.Equals(Error, StringComparison.OrdinalIgnoreCase)) && queueMsg.NumTimesProcessed < maxRetry) { return false; }

            return true;
        }
    }
}
