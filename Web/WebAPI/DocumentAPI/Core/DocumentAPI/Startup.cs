using DocumentAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;              // for HttpNoContentOutputFormatter
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using SimpleTokenProvider;
using Swashbuckle.Swagger.Model;
using System;
using System.IO;
using static DocumentAPI.Utils;

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
                Log.WriteLine("DocumentAPI started");

                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(Inv($"appsettings.{env.EnvironmentName}.json"), optional: true)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();

                // VERY IMPORTANT - get the private encryption key value from the environment variable where it is stored.
                // This must be set, or the service must shut down.
                _secretKey = Configuration.GetValue(typeof(string), "WebAPI_Private")?.ToString();
                Contract.Assert(_secretKey != null, "Failed to load required value from WebAPI_Private environment variable");

                InitializeDefaultValues();

                Utils.environment = env;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"));

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
                });

                services.Configure<ServerOptions>(Configuration);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"));
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
                Log.WriteLine(Inv($"Exception reported: {ex.Message}"));
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

            Utils.SetCurrentApiContext(databaseServer, databaseName, workflowName);
        }
    }
}
