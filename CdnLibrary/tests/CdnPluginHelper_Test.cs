/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    [TestClass]
    public class CdnPluginHelper_Test
    {
        [TestMethod]
        public void SplitRequestIntoBatches_MaxNum()
        {
            var urls = new HashSet<string>()
                {
                    "http://url1",
                    "http://url2",
                    "http://url3",
                    "http://url4"
                };

            int maxNum = 3;

            var result = CdnPluginHelper.SplitUrlListIntoBatches(urls, maxNum);

            Assert.AreEqual(2, result.Count);

            int i = 1;

            foreach (var r in result)
            {
                var remainingLength = 4 - i + 1; // Since i is not 0 based

                var batchSize = remainingLength < maxNum ? remainingLength : maxNum;

                Assert.AreEqual(batchSize, r.Length);

                foreach (var url in r)
                {
                    Assert.AreEqual($"http://url{i}", url);
                    i++;
                }
            }
        }

        [TestMethod]
        public void SplitRequestIntoBatches_LessThanBatchSize()
        {
            var urls = new HashSet<string>()
                {
                    "http://url1",
                    "http://url2",
                    "http://url3",
                    "http://url4"
                };

            int maxNum = 8;

            var result = CdnPluginHelper.SplitUrlListIntoBatches(urls, maxNum);

            Assert.AreEqual(1, result.Count);

            int i = 1;

            foreach (var r in result)
            {
                var remainingLength = 4 - i + 1; // Since i is not 0 based

                var batchSize = remainingLength < maxNum ? remainingLength : maxNum;

                Assert.AreEqual(batchSize, r.Length);

                foreach (var url in r)
                {
                    Assert.AreEqual($"http://url{i}", url);
                    i++;
                }
            }
        }

        [TestMethod]
        public void SplitRequestIntoBatches_EqualToBatchSize()
        {
            var urls = new HashSet<string>()
                {
                    "http://url1",
                    "http://url2",
                    "http://url3",
                    "http://url4"
                };

            int maxNum = 4;

            var result = CdnPluginHelper.SplitUrlListIntoBatches(urls, maxNum);

            Assert.AreEqual(1, result.Count);

            int i = 1;

            foreach (var r in result)
            {
                var remainingLength = 4 - i + 1; // Since i is not 0 based

                var batchSize = remainingLength < maxNum ? remainingLength : maxNum;

                Assert.AreEqual(batchSize, r.Length);

                foreach (var url in r)
                {
                    Assert.AreEqual($"http://url{i}", url);
                    i++;
                }
            }
        }

        [TestMethod]
        public void SplitRequestIntoBatches_NoUrl()
        {
            var urls = new HashSet<string>();

            int maxNum = 3;

            var result = CdnPluginHelper.SplitUrlListIntoBatches(urls, maxNum);

            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public void SplitRequestIntoBatches_ModMaxNum()
        {
            var urls = new HashSet<string>()
                {
                    "http://url1",
                    "http://url2",
                    "http://url3",
                    "http://url4"
                };

            int maxNum = 2;

            var result = CdnPluginHelper.SplitUrlListIntoBatches(urls, maxNum);

            Assert.AreEqual(maxNum, result.Count);

            int i = 1;

            foreach (var r in result)
            {
                Assert.AreEqual(maxNum, r.Length);

                foreach (var url in r)
                {
                    Assert.AreEqual($"http://url{i}", url);
                    i++;
                }
            }
        }

        [TestMethod]
        public void TryComputeHash_Success()
        {
            var data = Encoding.UTF8.GetBytes("Test Request Value");

            var hashString = CdnPluginHelper.ComputeHash(data, "SHA256");

            var algorithm = SHA256.Create();

            var hash = algorithm.ComputeHash(data);

            Assert.AreEqual(Convert.ToBase64String(hash), hashString);
        }

        [TestMethod]
        public void TryComputeHash_InvalidInput()
        {
            Assert.IsFalse(CdnPluginHelper.TryComputeHash(null, "SHA256", out _));
            Assert.IsFalse(CdnPluginHelper.TryComputeHash(new byte[2] { 1, 2 }, string.Empty, out _));
        }

        [TestMethod]
        public void TryComputeKeyedHash_Success()
        {
            var data = "Test Request Value";
            var key = "testkey";

            var hashString = CdnPluginHelper.ComputeKeyedHash(data, key, "HMACSHA256");

            var algorithm = new HMACSHA256(Encoding.UTF8.GetBytes(key));

            var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(data));

            Assert.AreEqual(Convert.ToBase64String(hash), hashString);
        }

        [TestMethod]
        public void TryComputeKeyedHash_InvalidInput()
        {
            Assert.IsFalse(CdnPluginHelper.TryComputeKeyedHash(null, null, "HMACSHA256", out _));
            Assert.IsFalse(CdnPluginHelper.TryComputeKeyedHash(Encoding.UTF8.GetBytes("test"), "test", null, out _));
        }
    }
}
