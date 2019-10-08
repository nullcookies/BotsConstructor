using System;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using DeleteMeWebhook;
using LogicalCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Telegram.Bot;

namespace Forest
{
    public static class Program
    {

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        
        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 8080);
                    options.Limits.MaxConcurrentConnections = 500;
                })
                .UseStartup<Startup>();
        }
    }
}




