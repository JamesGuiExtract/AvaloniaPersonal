using DocumentAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;              // for HttpNoContentOutputFormatter
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
using System;
using System.Collections.Generic;
using System.IO;
using static DocumentAPI.Controllers.UsersController;

namespace DocumentAPI
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// CTOR for Startup class
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            bool loggingEnabled = false;

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();

                //Set up logging now - hopefully nothing has gone wrong before this!
                string logPath = Configuration["LogPath"];
                if (string.IsNullOrEmpty(logPath))
                {
                    logPath = env.ContentRootPath + "\\Logs";
                }

                Log.WriteLine("DocumentAPI started");
                loggingEnabled = true;

                // For NUnit tests, the tests set these three configuration parameters,
                // AND the appsettings.json file can't be found. Rather than remove
                // the asserts from the properties, I've added these tests...
                var databaseServer = Configuration["DatabaseServer"];
                if (!string.IsNullOrEmpty(databaseServer))
                {
                    Utils.DatabaseServer = databaseServer;
                }

                var databaseName = Configuration["DatabaseName"];
                if (!string.IsNullOrEmpty(databaseName))
                {
                    Utils.DatabaseName = databaseName;
                }

                var attrSetName = Configuration["AttributeSetName"];
                if (!string.IsNullOrEmpty(attrSetName))
                { 
                    Utils.AttributeSetName = attrSetName;
                }

                var defaultWorkflow = Configuration["DefaultWorkflow"];
                if (!string.IsNullOrEmpty(defaultWorkflow))
                {
                    Utils.DefaultWorkflow = defaultWorkflow;
                }

                Utils.environment = env;
            }
            catch (Exception ex)
            {
                if (loggingEnabled)
                {
                    Log.WriteLine($"Exception reported: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Configuration property
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
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
                Log.WriteLine($"Exception reported: {ex.Message}");
            }
        }



        /// <summary>
        /// Configure method...
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                if (env.IsDevelopment())
                {
                }

                app.UseMvc();

                app.UseSwagger();
                app.UseSwaggerUi();

                var user = new User()
                {
                    Username = "admin",
                    Password = "a",
                    LoggedIn = false
                };

                AddMockUser(user);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Exception reported: {ex.Message}");
            }
        }
    }
}
