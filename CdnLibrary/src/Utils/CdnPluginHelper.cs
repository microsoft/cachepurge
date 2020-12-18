/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using CachePurgeLibrary;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;

    internal static class CdnPluginHelper
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true };

        internal static List<string[]> SplitUrlListIntoBatches(ISet<string> urlListToPurge, int batchSize)
        {
            var listOfBatches = new List<string[]>();

            batchSize = (urlListToPurge.Count <= batchSize) ? urlListToPurge.Count : batchSize;

            if (urlListToPurge.Count == 0)
            {
                return listOfBatches;
            }

            var batch = new string[batchSize];

            int i = 0, j = 0;

            foreach (var item in urlListToPurge)
            {
                if (j == batchSize)
                {
                    listOfBatches.Add(batch);

                    var remainingLength = urlListToPurge.Count - i;
                    var nextBatchSize = remainingLength < batchSize ? remainingLength : batchSize;
                    batch = new string[nextBatchSize];
                    j = 0;
                }

                batch[j] = item;
                i++;
                j++;
            }


            listOfBatches.Add(batch);

            return listOfBatches;
        }

        internal static string ComputeHash(ReadOnlySpan<byte> input, string hashType)
        {
            return TryComputeHash(input, hashType, out var hash) ? Convert.ToBase64String(hash) : string.Empty;
        }

        internal static bool IsValidRequest(IPartnerRequest partnerRequest)
        {
            if (partnerRequest != null && partnerRequest.Urls != null && string.IsNullOrEmpty(partnerRequest.Status) && !string.IsNullOrEmpty(partnerRequest.id))
            {
                return true;
            }
            return false;
        }

        internal static bool TryComputeHash(ReadOnlySpan<byte> input, string hashType, out ReadOnlySpan<byte> output)
        {
            output = null;

            if (input.IsEmpty || string.IsNullOrEmpty(hashType)) { return false; }

            using var algorithm = HashAlgorithm.Create(hashType);

            var dest = new byte[algorithm.HashSize].AsSpan();

            if (algorithm.TryComputeHash(input, dest, out int writtenBytes))
            {
                output = dest.Slice(0, writtenBytes);
                return true;
            }

            return false;
        }

        internal static string ComputeKeyedHash(string data, string key, string hashType)
        {
            return TryComputeKeyedHash(Encoding.UTF8.GetBytes(data), key, hashType, out var keyedHash) ? Convert.ToBase64String(keyedHash) : string.Empty;
        }

        internal static bool TryComputeKeyedHash(ReadOnlySpan<byte> input, string key, string hashType, out ReadOnlySpan<byte> keyedHash)
        {
            keyedHash = null;

            if (input.IsEmpty || string.IsNullOrEmpty(hashType) || string.IsNullOrEmpty(key)) { return false; }

            using var algorithm = HMAC.Create(hashType);
            algorithm.Key = Encoding.UTF8.GetBytes(key);

            keyedHash = algorithm.ComputeHash(input.ToArray());

            var dest = new byte[algorithm.HashSize].AsSpan();

            if (algorithm.TryComputeHash(input, dest, out int writtenBytes))
            {
                keyedHash = dest.Slice(0, writtenBytes);
                return true;
            }

            return false;
        }

        internal static RequestStatus GetRequestStatusFromResponseCode(HttpStatusCode result)
        {
            return result switch
            {
                HttpStatusCode.InternalServerError => RequestStatus.Error,
                HttpStatusCode.TooManyRequests => RequestStatus.Throttled,
                HttpStatusCode.Unauthorized => RequestStatus.Unauthorized,
                _ => RequestStatus.Unknown,
            };
        }
    }
}
