/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.Cosmos;
    using System.Threading.Tasks;

    public class CdnRequestTableManager : ICdnRequestTableManager<CDN>
    {
        private readonly IRequestTable<AfdRequest> afdRequestTable;
        private readonly IRequestTable<AkamaiRequest> akamaiRequestTable;


        public CdnRequestTableManager()
        {
            afdRequestTable = new AfdRequestTable();
            akamaiRequestTable = new AkamaiRequestTable();
        }

        public CdnRequestTableManager(Container afdRequestContainer, Container akamaiRequestContainer)
        {
            afdRequestTable = new AfdRequestTable(afdRequestContainer);
            akamaiRequestTable = new AkamaiRequestTable(akamaiRequestContainer);
        }

        public async Task CreateCdnRequest(ICdnRequest request, CDN cdn)
        {
            if (cdn == CDN.AFD)
            {
                await afdRequestTable.CreateItem(request as AfdRequest);
            }
            else if (cdn == CDN.Akamai)
            {
                await akamaiRequestTable.CreateItem(request as AkamaiRequest);
            }
        }

        public async Task UpdateCdnRequest(ICdnRequest cdnRequest, CDN cdn)
        {
            if (cdn == CDN.AFD)
            {
                await afdRequestTable.UpsertItem(cdnRequest as AfdRequest);
            }
            else if (cdn == CDN.Akamai)
            {
                await akamaiRequestTable.UpsertItem(cdnRequest as AkamaiRequest);
            }
        }

        public void Dispose()
        {
            afdRequestTable.Dispose();
            akamaiRequestTable.Dispose();
        }
    }
}