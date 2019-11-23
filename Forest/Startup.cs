using System;
using System.Runtime.InteropServices;
using DataLayer;
using Forest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLibrary;

namespace Forest
{
    public class Startup
    {
        private IHostingEnvironment _environment;
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        private IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(DbContextFactory.GetConnectionString()))
                .BuildServiceProvider();

			services.AddSingleton<DbConnector>();
            services.AddSingleton<SimpleLogger>();
            services.AddSingleton<BotStatisticsSynchronizer>();
            services.AddSingleton<RouteRecordsSynchronizerService>();



        }

        public void Configure(IApplicationBuilder app,
            BotStatisticsSynchronizer botStatisticsSynchronizer,
            SimpleLogger logger,
            RouteRecordsSynchronizerService routeRecordsSynchronizerService)
        {

                
            logger.Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                "Запуск сервера леса");

            botStatisticsSynchronizer.Start();
            routeRecordsSynchronizerService.Start();
            
            
            app.UseDeveloperExceptionPage();
            

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            logger.Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                "Запуск сервера леса закончен");
        }
    }
}
