using DocumentAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;              // for HttpNoContentOutputFormatter
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Utils.DatabaseServer = Configuration["DatabaseServer"];

            Utils.DatabaseName = Configuration["DatabaseName"];

            Utils.AttributeSetName = Configuration["AttributeSetName"];

            Utils.environment = env;
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

        

        /// <summary>
        /// Configure method...
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string logPath = env.ContentRootPath + "\\Logs";
            Log.Create(logPath);

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
    }
}
