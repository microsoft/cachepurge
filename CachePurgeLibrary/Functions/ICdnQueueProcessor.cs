/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CachePurgeLibrary
{
    using Microsoft.Azure.Storage.Queue;
    using System.Threading.Tasks;

    internal interface ICdnQueueProcessor<T>
    {
        Task<CloudQueueMessage> ProcessPollRequest(T queueMsg, int maxRetry);

        CloudQueueMessage CompletePollRequest(IRequestInfo requestInfo, T queueMsg);

        Task<CloudQueueMessage> ProcessPurgeRequest(T queueMsg, int maxRetry);

        CloudQueueMessage CompletePurgeRequest(IRequestInfo requestInfo, T queueMsg);

        void AddMessageToQueue(CloudQueue outputQueue, CloudQueueMessage message, T queueMsg);

        CloudQueueMessage CreateCloudQueueMessage(T request, IRequestInfo requestInfo);
    }
}
