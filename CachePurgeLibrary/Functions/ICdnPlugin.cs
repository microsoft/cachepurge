/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using Microsoft.Azure.WebJobs;
    using System.Collections.Generic;

    public interface ICdnPlugin<T>
    {
        bool ProcessPartnerRequest(T partnerRequest, ICollector<ICdnRequest> queue);

        void AddMessagesToSendQueue(ICdnRequest cdnRequest, ICollector<ICdnRequest> msg);

        IList<ICdnRequest> SplitRequestIntoBatches(T partnerRequest, int maxNumUrl);

        bool ValidPartnerRequest(string inputRequest, string resourceID, out T partnerRequest);
    }
}
