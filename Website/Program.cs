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
            try
            { 

                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                string writePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                writePath = Path.Combine(writePath, "mydich.txt");

                using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(e.Message);
                }
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
