using Extract;
using Swashbuckle.AspNetCore.Swagger;
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
        /// <param name="operation">Swagger operation</param>
        /// <param name="context">unused</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            try
            {
                // “api” + [Controller name] + [Method Name] + [HTTP Verb]
                // Or in other words, it is the URL of your controller method, but without “/” + [HTTP Verb]
                //            if (operation.OperationId.ToLower() == "apidocumentuploadfilepost")
                if (operation.OperationId.IsEquivalent("ApiDocumentPost"))
                {
                    // Clear all parameters EXCEPT for the authorization parameter, if it exists. Not doing this
                    // creates an order dependency - the auth filter needs to be AFTER the file upload filter. If
                    // the auth filter is removed here, then Swagger won't correctly generate code for SubmitFile.
                    var operationParameters = new List<IParameter>(operation.Parameters);
                    operation.Parameters.Clear();

                    operation.Parameters.Add(new NonBodyParameter
                    {
                        Name = "file",      // Name parameter value must be equal to parameter name of the method
                        In = "formData",
                        Description = "Upload File",
                        Required = true,
                        Type = "file"
                    });

                    operation.Consumes.Add("multipart/form-data");

                    // For some unknown reason, it seems to be important to add the auth header back to the list
                    // after the file upload filter. 
                    var index = operationParameters.FindIndex(p => p.Name.IsEquivalent("Authorization"));
                    if (index >= 0)
                    {
                        operation.Parameters.Add(operationParameters[index]);
                    }
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
