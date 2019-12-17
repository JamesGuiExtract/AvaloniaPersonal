using Extract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Formatters;              // for HttpNoContentOutputFormatter
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using WebAPI.Filters;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI
{
    /// <summary>
    /// Startup class
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// CTOR for Startup class
        /// </summary>
        /// <param name="env">hosting environment</param>
        public Startup(IHostingEnvironment env)
        {
            try
            {
                Log.WriteLine("DocumentAPI started", "ELI43258");

                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(Inv($"appsettings.{env.EnvironmentName}.json"), optional: true)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();

                InitializeDefaultValues();

                Utils.environment = env;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.AsExtract("ELI43650"));

                // Any exception here should cause the service to not start.
                throw;
            }
        }

        /// <summary>
        /// Configuration property
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services">collection of services</param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // In order to allow one web page (such as a web application) to in turn call this
                // web-service, cross-origin requests (CORS) are required.
                // https://docs.microsoft.com/en-us/aspnet/core/security/cors
                // https://stackoverflow.com/questions/44379560/how-to-enable-cors-in-asp-net-core-webapi
                // https://extract.atlassian.net/browse/ISSUE-15077
                services.AddCors(
                    options => options.AddPolicy("AllowAll",
                        builder => builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()));

                // The ApplicationPartManager and ActionContextAccessor are need by Swashbuckle/Swagger
                var manager = new ApplicationPartManager();
                manager.ApplicationParts.Add(new AssemblyPart(typeof(Startup).Assembly));
                services.AddSingleton(manager);
                services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
                services.AddMvc();

                // Register Swashbuckle/Swagger - auto-generate API documentation
                var basepath = PlatformServices.Default.Application.ApplicationBasePath;

                // TODO - when project is renamed, this xml file name must be changed.
                var xmlPath = Path.Combine(basepath, "DocumentAPI.xml");
                Log.WriteLine(Inv($"Swagger file: {xmlPath}"), "ELI43368");

                services.AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("DocumentAPI",
                        new Info
                        {
                            Title = "Extract Document API",
                            Description = "API support to enable integration of Extract processing with external workflows.",
                            Contact = new Contact
                            {
                                Name = "Extract Systems",
                                Email = "developers@extractsystems.com",
                                Url = "http://www.extractsystems.com"
                            },
                            Version = "2.0",
                            License = new License
                            {
                                Name = "Use under Extract Systems license",
                                Url = "http://extractsystems.com"
                            }
                        });

                    config.IncludeXmlComments(xmlPath);
                    config.DescribeAllEnumsAsStrings();

                    // Enables swagger gen to recognize [Authorize] attributes on controllers and actions
                    config.OperationFilter<AuthorizationHeaderParameterOperationFilter>();

                    // Register File Upload Operation Filter - this supports upload button in Swagger UI for Document.SubmitFile
                    config.OperationFilter<FileUploadOperation>();

                    // Updates Content-Type list for Consumes and Produces; partially to avoid having to see Consumes/Produces
                    // attributes for every method and partially because there seems to otherwise be no good way to clear the
                    // lists via Produces/Consumes attributes when no content is produced or consumed.
                    config.OperationFilter<ContentTypeSpecifier>();

                    // Mapping FileResult to "file" causes NSwag's automatic client generation to use a return type
                    // of FileResponse for methods that return PhysicalFileResult. On the client side, FileResponse allows
                    // for the streaming of the file itself whereas PhysicalFileResult provides only the source paths.
                    config.MapType<FileResult>(() => new Schema { Type = "file", });
                });

                services.Configure<ServerOptions>(Configuration);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"), "ELI43260");
            }
        }

        /// <summary>
        /// Configure method...
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">hosting environment</param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                /*
                if (env.IsDevelopment())
                {
                }
                */

                // configure authorization and token creation this server provides
                ConfigureAuth(app);

                app.UseSwagger();
                app.UseSwaggerUI(config =>
                {
                    config.RoutePrefix = "documentation";
                    config.DocumentTitle = "Extract Document API";
                    config.SwaggerEndpoint("/swagger/DocumentAPI/swagger.json", "DocumentAPI");
                });

                app.UseMvc();
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"), "ELI43261");
            }
        }

        /// <summary>
        /// Initialize settings from the appsettings.json config file
        /// </summary>
        void InitializeDefaultValues()
        {
            var databaseServer = Configuration["DatabaseServer"];
            var databaseName = Configuration["DatabaseName"];
            var workflowName = Configuration["DefaultWorkflow"];

            var dbConnectionRetries = Configuration["DbNumberOfConnectionRetries"];
            var dbConnectionTimeout = Configuration["DbConnectionRetryTimeout"];

            var exceptionLogFilter = Configuration["ExceptionLogFilter"];

            Utils.SetCurrentApiContext(databaseServer, databaseName, workflowName, dbConnectionRetries, dbConnectionTimeout, exceptionLogFilter);

            Utils.ValidateCurrentApiContext();

            var timeoutValue = Configuration["TokenTimeoutSeconds"];
            if (!String.IsNullOrWhiteSpace(timeoutValue))
            {
                bool parsed = Int32.TryParse(timeoutValue, out int timeoutInSecondsValue);
                if (parsed)
                {
                    AuthUtils.TokenTimeoutInSeconds = timeoutInSecondsValue;                    
                }
                else
                {
                    Log.WriteLine(Inv($"Warning: the text: {timeoutValue}, could not be parsed ") +
                                      "as a TokenTimeoutSeconds value, so the default value has been applied", "ELI43341");
                }
            }
        }
    }
}
