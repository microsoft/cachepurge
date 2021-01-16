// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi.Swagger
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class PostContentAttribute : Attribute
    {
        public PostContentAttribute(string name, string description, string example = "")
        {
            this.Name = name;
            this.Description = description;
            this.Example = example;
        }

        public string Name { get; }

        public string Description { get; set; }

        public string Example { get; set; }
    }
}