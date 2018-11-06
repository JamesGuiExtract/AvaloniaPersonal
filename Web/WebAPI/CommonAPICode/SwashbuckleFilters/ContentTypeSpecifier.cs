using Extract;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

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
                if (operation.OperationId == "ApiDocumentByIdGet")
                {
                    operation.Produces.Clear();
                    operation.Produces.Add("image/tiff");
                    operation.Produces.Add("application/pdf");
                }
                else if (operation.OperationId == "ApiDocumentByIdOutputFileGet")
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

                if (operation.OperationId == "ApiDocumentPost")
                {
                    operation.Consumes.Clear();
                    operation.Consumes.Add("multipart/form-data");
                }
                else if (operation.OperationId == "ApiDocumentTextPost")
                {
                    operation.Consumes.Clear();
                    operation.Consumes.Add("text/plain");
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
