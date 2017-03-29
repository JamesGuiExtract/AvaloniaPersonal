using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace DocumentAPI
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
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
