using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebAPI
{
    /// <summary>
    /// Program class, with method Main()
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main - program entry point
        /// </summary>
        /// <param name="args">currently no supported command line arguments</param>
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                // UseUrls might be important for CORs? Can't remember reason for adding it...
                .UseUrls("http://localhost:80")
                // Loading configuration allows specifying the urls to listen on with the appsettings.json:
                //   "urls": "http://192.168.44.144:80;"
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
