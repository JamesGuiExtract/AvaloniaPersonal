using Extract;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace WebAPI.Filters
{
    /// <summary>
    /// SwaggerGenOptions.DescribeAllEnumsAsStrings() is obsolete in Swagger v5+; it is supposed
    /// to use System.Text.Json for serialization and that serialization includes specification
    /// for enum translation. However, this functionality appears to depend on Asp.Net Core 3.0,
    /// so it appears for now a filter is necessary.
    /// </summary>
    public class EnumsAsStringFilter : ISchemaFilter
    {
        /// <summary>
        /// Replace enum integer value with string name
        /// </summary>
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            try
            {
                if (context.Type.IsEnum)
                {
                    model.Enum.Clear();
                    Enum.GetNames(context.Type)
                        .ToList()
                        .ForEach(value => model.Enum.Add(new OpenApiString(value)));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51492");
            }
        }
    }
}
