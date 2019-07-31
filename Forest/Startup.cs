using DeleteMeWebhook.Models;
using DeleteMeWebhook.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using DataLayer.Models;
using DataLayer.Services;
using System.Runtime.InteropServices;

namespace DeleteMeWebhook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            string connection;

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                connection = Configuration.GetConnectionString("PostgresConnectionDevelopment");
            }
            else
            {
                connection = Configuration.GetConnectionString("PostgresConnectionLinux");
            }

            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(connection)).BuildServiceProvider();

			services.AddSingleton<DBConnector>();
            services.AddSingleton<StupidLogger>();


        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            //app.UseHttpsRedirection();

            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024

            };

            app.UseWebSockets(wsOptions);




            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
