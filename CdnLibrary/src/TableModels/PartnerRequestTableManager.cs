/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using Microsoft.Azure.Cosmos;
    using System.Threading.Tasks;

    public class PartnerRequestTableManager : IPartnerRequestTableManager<CDN>
    {
        private readonly IRequestTable<AfdPartnerRequest> afdPartnerRequestTable;
        private readonly IRequestTable<AkamaiPartnerRequest> akamaiPartnerRequestTable;


        public PartnerRequestTableManager()
        {
            this.afdPartnerRequestTable = new AfdPartnerRequestTable();
            this.akamaiPartnerRequestTable = new AkamaiPartnerRequestTable();
        }

        public PartnerRequestTableManager(Container afdPartnerRequestContainer, Container akamaiPartnerRequestContainer)
        {
            this.afdPartnerRequestTable = new AfdPartnerRequestTable(afdPartnerRequestContainer);
            this.akamaiPartnerRequestTable = new AkamaiPartnerRequestTable(akamaiPartnerRequestContainer);
        }

        public async Task CreatePartnerRequest(IPartnerRequest partnerRequest, CDN cdn)
        {
            if (cdn == CDN.AFD)
            {
                await afdPartnerRequestTable.CreateItem(partnerRequest as AfdPartnerRequest);
            }
            else if (cdn == CDN.Akamai)
            {
                await akamaiPartnerRequestTable.CreateItem(partnerRequest as AkamaiPartnerRequest);
            }
        }

        public async Task UpdatePartnerRequest(IPartnerRequest partnerRequest, CDN cdn)
        {
            if (cdn == CDN.AFD)
            {
                await afdPartnerRequestTable.UpsertItem(partnerRequest as AfdPartnerRequest);
            }
            else if (cdn == CDN.Akamai)
            {
                await akamaiPartnerRequestTable.UpsertItem(partnerRequest as AkamaiPartnerRequest);
            }
        }

        public async Task<IPartnerRequest> GetPartnerRequest(string id, CDN cdn)
        {
            if (cdn == CDN.AFD)
            {
                return await afdPartnerRequestTable.GetItem(id);
            }
            else if (cdn == CDN.Akamai)
            {
                return await akamaiPartnerRequestTable.GetItem(id);
            }
            return null;
        }

        public void Dispose()
        {
            afdPartnerRequestTable.Dispose();
            akamaiPartnerRequestTable.Dispose();
        }
    }
}