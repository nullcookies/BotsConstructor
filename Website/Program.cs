using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Website
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Намана");
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            // var host = new WebHostBuilder()
            //     .UseKestrel()
            //     .UseContentRoot(Directory.GetCurrentDirectory())
            //     // .UseIISIntegration()
            //     .UseStartup<Startup>()
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("https://localhost:5001/");
        }
    }
}
