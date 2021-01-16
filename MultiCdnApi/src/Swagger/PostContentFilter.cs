// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi.Swagger
{
    using System.Linq;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class PostContentFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var postContentAttribute = (PostContentAttribute) context.MethodInfo.GetCustomAttributes(true)
                .SingleOrDefault(attribute => attribute is PostContentAttribute);
            if (postContentAttribute != null)
            {
                operation.RequestBody = new OpenApiRequestBody();
                operation.RequestBody.Content.Add(postContentAttribute.Name,
                    new OpenApiMediaType {
                        Schema = new OpenApiSchema {
                            Type = "multipart/form-data", 
                            Description = postContentAttribute.Description,
                            Example = new OpenApiString(postContentAttribute.Example)
                        }
                    });
            }
        }
    }
}