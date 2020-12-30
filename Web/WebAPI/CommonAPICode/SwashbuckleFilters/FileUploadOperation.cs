using Extract;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace WebAPI.Filters
{
    /// <summary>
    /// Implementing this filter will provide the option to modify or replace the operation parameters.
    /// This is used to get Document.SubmitFile to show an Upload file button in the Swagger UI.
    /// </summary>
    public class FileUploadOperation: IOperationFilter
    {
        /// <summary>
        /// This filter will replace the multiple input boxes with file upload control
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            try
            {
                if (context.MethodInfo.Name == "PostDocument")
                {
                    // Clear all parameters and add a request body for field for providing document input.
                    var operationParameters = new List<OpenApiParameter>(operation.Parameters);
                    operation.Parameters.Clear();

                    operation.RequestBody = new OpenApiRequestBody()
                    {
                        Content =
                        {
                            ["multipart/form-data"] = new OpenApiMediaType()
                            {
                                Schema = new OpenApiSchema()
                                {
                                    Type = "object",
                                    Properties =
                                    {
                                        ["file"] = new OpenApiSchema()
                                        {
                                            Type = "file",
                                            Format = "binary"
                                        }
                                    }
                                }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43364");
                Log.WriteLine(ee);

                throw ee;
            }
        }
    }
}
