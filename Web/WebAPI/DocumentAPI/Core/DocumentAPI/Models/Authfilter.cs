using Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Microsoft.AspNetCore.Mvc.Authorization;


namespace DocumentAPI.Models
{
    /// <summary>
    /// class to add a special authorization filter that allows Swagger 
    /// to understand [Authorize] attributes on a controller.
    /// </summary>
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Apply 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            try
            {
                var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
                var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
                var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

                if (isAuthorized && !allowAnonymous)
                {
                    if (operation.Parameters == null)
                    {
                        operation.Parameters = new List<IParameter>();
                    }

                    operation.Parameters.Add(new NonBodyParameter
                    {
                        Name = "Authorization",
                        In = "header",
                        Description = "access token",
                        Required = true,
                        Type = "string"
                    });
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43199");
                Log.WriteLine(ee);

                throw ee;
            }
        }
    }
}
