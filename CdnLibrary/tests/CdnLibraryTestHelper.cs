/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary_Test
{
    using CachePurgeLibrary;
    using Microsoft.Azure.Cosmos;
    using Moq;
    using System.Collections.Generic;
    using System.Threading;

    public static class CdnLibraryTestHelper
    {
        public static Container MockCosmosDbContainer<T>(IDictionary<string, T> containerContents) where T: ICosmosDbEntity
        {
            var responseMock = new Mock<ItemResponse<T>>();

            var containerMock = new Mock<Container>();
            containerMock
                .Setup(c => c.CreateItemAsync(
                        It.IsAny<T>(),
                        It.IsAny<PartitionKey?>(),
                        It.IsAny<ItemRequestOptions>(),
                        It.IsAny<CancellationToken>())
                    )
                    .Callback<T, PartitionKey?, ItemRequestOptions, CancellationToken>((p, x, y, z) => containerContents.Add(p.id, p))
                    .ReturnsAsync(responseMock.Object);

            containerMock
                .Setup(c => c.UpsertItemAsync(
                        It.IsAny<T>(),
                        It.IsAny<PartitionKey?>(),
                        It.IsAny<ItemRequestOptions>(),
                        It.IsAny<CancellationToken>())
                    )
                    .Callback<T, PartitionKey?, ItemRequestOptions, CancellationToken>((p, x, y, z) => containerContents[p.id] = p)
                    .ReturnsAsync(responseMock.Object);

            var feedResponseMock = new Mock<FeedResponse<T>>();

            feedResponseMock.Setup(x => x.Count).Returns(() => containerContents.Count);
            feedResponseMock.Setup(x => x.GetEnumerator()).Returns(() => containerContents.Values.GetEnumerator());

            var feedIteratorMock = new Mock<FeedIterator<T>>();
            feedIteratorMock
                .Setup(f => f.HasMoreResults)
                    .Returns(true);

            feedIteratorMock
                .Setup(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(feedResponseMock.Object)
                    .Callback(() => feedIteratorMock
                    .Setup(f => f.HasMoreResults)
                    .Returns(false));

            containerMock
                .Setup(c => c.GetItemQueryIterator<T>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(feedIteratorMock.Object);

            return containerMock.Object;
        }
    }
}
