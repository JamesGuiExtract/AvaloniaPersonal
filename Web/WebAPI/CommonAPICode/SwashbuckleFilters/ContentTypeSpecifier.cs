using Extract;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebAPI.Filters
{
    /// <summary>
    /// Updates Content-Type list for Consumes and Produces; partially to avoid having to see Consumes/Produces
    /// attributes for every method and partially because there seems to otherwise be no good way to clear the
    /// lists via Produces/Consumes attributes when no content is produced or consumed.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class ContentTypeSpecifier : IOperationFilter
    {
        /// <summary>
        /// Updates the Content-Type list for Consumes and Produces.
        /// </summary>
        /// <param name="operation">The <see cref="Operation"/></param>
        /// <param name="context">The <see cref="OperationFilterContext"/></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            try
            {
                if (Regex.IsMatch(operation.OperationId, @"(?ix)Api(V\d+\.\d+)?DocumentByIdGet"))
                {
                    operation.Produces.Clear();
                    operation.Produces.Add("image/tiff");
                    operation.Produces.Add("application/pdf");
                }
                else if (Regex.IsMatch(operation.OperationId, @"(?ix)Api(V\d +\.\d +)?DocumentByIdOutputFileGet"))
                {
                    operation.Produces.Clear();
                    operation.Produces.Add("image/tiff");
                    operation.Produces.Add("application/pdf");
                    operation.Produces.Add("application/xml");
                    operation.Produces.Add("application/octet-stream");
                }
                else if (context.ApiDescription.SupportedResponseTypes.Any(type => type.StatusCode == 204))
                {
                    operation.Produces.Clear();
                }
                else
                {
                    operation.Produces.Clear();
                    operation.Produces.Add("application/json");
                }

                if (Regex.IsMatch(operation.OperationId, @"(?ix)Api(V\d+\.\d+)?DocumentPost"))
                {
                    operation.Consumes.Clear();
                    operation.Consumes.Add("multipart/form-data");
                }
                else if (Regex.IsMatch(operation.OperationId, @"(?ix)Api(V\d+\.\d+)?DocumentTextPost"))
                {
                    operation.Consumes.Clear();

                    // Starting in version 3.0, DocumentTextPost should report a consumed content type
                    // of "multipart/form-data; charset=utf-8"
                    if (!context.ApiDescription.TryGetMethodInfo(out var methodInfo)
                        || methodInfo.DeclaringType.GetCustomAttributes(true)
                            .OfType<ApiVersionAttribute>()
                            .SelectMany(c => c.Versions)
                            .Any(v => v.MajorVersion >= 3))
                    {
                        operation.Consumes.Add("multipart/form-data; charset=utf-8");
                    }
                    else
                    {
                        // The original API versions reported a consumed content type of "text/plain"
                        operation.Consumes.Add("text/plain");
                    }
                }
                else if (context.ApiDescription.HttpMethod != "GET")
                {
                    operation.Consumes.Clear();
                    operation.Consumes.Add("application/json");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46460");
            }
        }

        /// <summary>
        /// Translates the type of the content.
        /// </summary>
        /// <param name="contentTypeList">The content type list.</param>
        static void TranslateContentType(IList<string> contentTypeList)
        {
            if (contentTypeList.Count == 1)
            {
                if (contentTypeList[0] == "image/tiff")
                {
                    contentTypeList.Clear();
                    contentTypeList.Add("image/tiff");
                    contentTypeList.Add("application/pdf");
                }
                else if (contentTypeList[0] == "application/octet-stream")
                {
                    contentTypeList.Clear();
                    contentTypeList.Add("image/tiff");
                    contentTypeList.Add("application/pdf");
                    contentTypeList.Add("application/xml");
                    contentTypeList.Add("application/octet-stream");
                }
                else if (contentTypeList[0] == "application/*")
                {
                    contentTypeList.Clear();
                }
            }
        }
    }
}
