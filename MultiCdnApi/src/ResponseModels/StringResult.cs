/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using Microsoft.AspNetCore.Mvc;

    public class StringResult: JsonResult
    {
        public StringResult(string str) : base(str) {}
    }
}