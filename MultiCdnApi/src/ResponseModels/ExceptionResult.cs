/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace MultiCdnApi
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public class ExceptionResult: JsonResult
    {
        public ExceptionResult(Exception exception) : base(exception) {}
    }
}