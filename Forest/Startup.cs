using System.Runtime.InteropServices;
using DataLayer;
using DataLayer.Models;
using DeleteMeWebhook.Services;
using Forest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var connection = Configuration.GetConnectionString(isWindows ? "PostgresConnectionWindows" : "PostgresConnectionLinux");

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(connection))
                .BuildServiceProvider();

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
