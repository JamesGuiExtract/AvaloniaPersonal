using DocumentAPI.Models;
using Extract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;              // for HttpNoContentOutputFormatter
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
using System;
using System.IO;

using static DocumentAPI.Utils;
using DocumentAPI.Controllers;
using DocumentAPI.Filters;

namespace DocumentAPI
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
        /// </summary>140
        /// 
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
                // Add framework services.
                services.AddMvc(options =>
                {
                    // Remove the HttpNoContentOutputFormatter, so that null object will be represented
                    // in JSON (as "null"), instead of returning http error "204 No Content" response
                    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
                });

                // Add the API Model
                //services.AddSingleton<IFileItemRepository, FileItemRepository>();

                // Register Swashbuckle/Swagger - auto-generate API documentation
                var basepath = PlatformServices.Default.Application.ApplicationBasePath;

                // TODO - when project is renamed, this xml file name must be changed.
                var xmlPath = Path.Combine(basepath, "DocumentAPI.xml");
                Log.WriteLine(Inv($"Swagger file: {xmlPath}"), "ELI43368");

                services.AddSwaggerGen();
                services.ConfigureSwaggerGen(options =>
                {
                    options.SingleApiVersion(new Info
                    {
                        Version = "v1",
                        Title = "Document API",
                        Description = "Extract Document API documentation",
                        TermsOfService = "None",
                        Contact = new Contact
                        {
                            Name = "Extract developers team",
                            Email = "developers@extractsystems.com",
                            Url = "http://www.extractsystems.com"
                        },
                        License = new License
                        {
                            Name = "Use under Extract Systems license",
                            Url = "http://extractsystems.com"
                        }
                    });

                    options.IncludeXmlComments(xmlPath);
                    options.DescribeAllEnumsAsStrings();

                    // Enables swagger gen to recognize [Authorize] attributes on controllers and actions
                    options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();

                    // Register File Upload Operation Filter - this supports upload button in Swagger UI for Document.SubmitFile
                    options.OperationFilter<FileUploadOperation>();
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

                app.UseMvc();

                app.UseSwagger();
                app.UseSwaggerUi();
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
            // VERY IMPORTANT - get the private encryption key value from the environment variable where it is stored.
            // This must be set, or the service must shut down.
            // NOTE: The more elaborate:
            //_secretKey = Configuration.GetValue(typeof(string), "WebAPI_Private")?.ToString();
            // has been documented as being capable of reading values of environment variables. However I have discovered
            // that the indexer method just reads the value from whatever available configuration source has been
            // configured. So the easier-to-read indexer method will get the value from appsettings, or env.
            _secretKey = Configuration["WebAPI_Private"];
            Contract.Assert(!String.IsNullOrWhiteSpace(_secretKey), "Failed to load required value from WebAPI_Private environment variable");

            var databaseServer = Configuration["DatabaseServer"];
            var databaseName = Configuration["DatabaseName"];
            var workflowName = Configuration["DefaultWorkflow"];

            var dbConnectionRetries = Configuration["DbNumberOfConnectionRetries"];
            var dbConnectionTimeout = Configuration["DbConnectionRetryTimeout"];

            Utils.SetCurrentApiContext(databaseServer, databaseName, workflowName, dbConnectionRetries, dbConnectionTimeout);

            // "Applying" the context values at startup is a convenient way of ensuring that the configured context is
            // correct, as any problems will be asserted and logged, and the service will exit from starting up, so it
            // will be obvious that there is a configuration issue. Apply is the default behavior; it can be explicitly
            // switched off.
            var applyContextOnStartup = Configuration["ApplyContextOnStartup"];
            if (String.IsNullOrWhiteSpace(applyContextOnStartup) || applyContextOnStartup.IsEquivalent("true"))
            {
                Utils.ApplyCurrentApiContext();
            }

            var timeoutValue = Configuration["TokenTimeoutSeconds"];
            if (!String.IsNullOrWhiteSpace(timeoutValue))
            {
                bool parsed = Int32.TryParse(timeoutValue, out int timeoutInSecondsValue);
                if (parsed)
                {
                    UsersController.TokenTimeoutInSeconds = timeoutInSecondsValue;                    
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
