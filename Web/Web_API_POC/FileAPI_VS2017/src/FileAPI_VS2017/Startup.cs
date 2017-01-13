using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using Swashbuckle.Swagger.Model;

namespace FileAPI_VS2017
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
            services.AddMvc();

            // Add the API Model
            services.AddSingleton<IFileItemRepository, FileItemRepository>();

            // Register Swashbuckle/Swagger - auto-generate API documentation
            var basepath = PlatformServices.Default.Application.ApplicationBasePath;

            // TODO - when project is renamed, this xml file name must be changed.
            var xmlPath = Path.Combine(basepath, "FileAPI_VS2017.xml");

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "File API",
                    Description = "File API documentation",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "Extract developers team",
                        Email = "developers.extractsystems.com",
                        Url = "http://www.extractsystems.com"
                    },
                    License = new License
                    {
                        Name = "Use under Extract Systems license",
                        Url = "http://extractsystems.com"
                    }
                });
                options.IncludeXmlComments(xmlPath);
            });

            // This used to work with VS 2015 and Swashbuckle v6.0.0.-beta902,
            // which is the same version I'm using here with VS 2017!
            /*
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(xmlPath);
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Aristotle API",
                    Version = "v1",
                    Description = "Web API Documentation",
                    Contact = new Contact
                    {
                        Name = "Extract developers team",
                        Email = "developers@extractsystems.com",
                        Url = "http://www.extractsystems.com"
                    }
                });
            });
            */

            services.AddSwaggerGen();
        }

        /// <summary>
        /// Configure method...
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
