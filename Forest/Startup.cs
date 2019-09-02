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
using Forest.Services;

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
            services.AddSingleton<BotStatisticsSynchronizer>();
            services.AddSingleton<BannedUsersSynchronizer>();



        }

        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            BotStatisticsSynchronizer botStatisticsSynchronizer,
            StupidLogger logger,
            BannedUsersSynchronizer bannedUsersSynchronizer)
        {

            logger.Log(LogLevelMyDich.IMPORTANT_INFO,
                Source.WEBSITE,
                "Запуск сервера леса");


            app.UseDeveloperExceptionPage();

            //app.UseHttpsRedirection();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
