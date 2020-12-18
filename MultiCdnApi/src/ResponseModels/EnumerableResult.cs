/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    public class EnumerableResult<T>: JsonResult where T : JsonResult
    {
        public EnumerableResult(IEnumerable<T> values) : base(values) {}
    }
}