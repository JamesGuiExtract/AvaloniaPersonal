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
                Log.WriteLine("AppBackendAPI started", "ELI45078");

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
                Log.WriteLine(ex.AsExtract("ELI45079"));

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
                services.AddMvc(options =>
                {
                    // Remove the HttpNoContentOutputFormatter, so that null object will be represented
                    // in JSON (as "null"), instead of returning http error "204 No Content" response
                    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
                });

                // Register Swashbuckle/Swagger - auto-generate API documentation
                var basepath = PlatformServices.Default.Application.ApplicationBasePath;

                // TODO - when project is renamed, this xml file name must be changed.
                var xmlPath = Path.Combine(basepath, "AppBackendAPI.xml");
                Log.WriteLine(Inv($"Swagger file: {xmlPath}"), "ELI45080");

                services.AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("AppBackendAPI", 
                        new Info
                        {
                            Title = "Application Backend API",
                            Description = "Backend API support for web-based verification applications.",
                            Contact = new Contact
                            {
                                Name = "Extract Systems",
                                Email = "developers@extractsystems.com",
                                Url = "http://www.extractsystems.com"
                            },
                            Version = "1.0",
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

                    // Mapping FileResult to "file" causes NSwag's automatic client generation to use a return type
                    // of FileResponse for methods that return PhysicalFileResult. On the client side, FileResponse allows
                    // for the streaming of the file itself whereas PhysicalFileResult provides only the source paths.
                    config.MapType<FileResult>(() => new Schema { Type = "file", });
                });

                services.Configure<ServerOptions>(Configuration);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"), "ELI45081");
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
                    config.DocumentTitle = "Extract Application Backend API";
                    config.SwaggerEndpoint("/swagger/AppBackendAPI/swagger.json", "AppBackendAPI");
                });

                app.UseMvc();
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"), "ELI45082");
            }
        }

        /// <summary>
        /// Initialize settings from the appsettings.json config file
        /// </summary>
        void InitializeDefaultValues()
        {
            var apiVersion = Configuration["ApiVersion"];
            var databaseServer = Configuration["DatabaseServer"];
            var databaseName = Configuration["DatabaseName"];
            var workflowName = Configuration["DefaultWorkflow"];

            var dbConnectionRetries = Configuration["DbNumberOfConnectionRetries"];
            var dbConnectionTimeout = Configuration["DbConnectionRetryTimeout"];

            Utils.SetCurrentApiContext(apiVersion, databaseServer, databaseName, workflowName, dbConnectionRetries, dbConnectionTimeout);

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
                                      "as a TokenTimeoutSeconds value, so the default value has been applied", "ELI45083");
                }
            }
        }
    }
}
