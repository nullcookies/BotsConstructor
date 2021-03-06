﻿using System;
using System.Threading.Channels;
using DataLayer;
using Forest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLibrary;

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

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(DbConnectionGlobals.GetConnectionString()))
                .BuildServiceProvider();

			services.AddSingleton<DbConnector>();
            services.AddSingleton<SimpleLogger>();
            services.AddSingleton<BotStatisticsSynchronizer>();
            services.AddSingleton<RouteRecordsSynchronizerService>();
        }

        public void Configure(IApplicationBuilder app, BotStatisticsSynchronizer botStatisticsSynchronizer, SimpleLogger logger, RouteRecordsSynchronizerService routeRecordsSynchronizerService)
        {
            logger.Log(LogLevel.IMPORTANT_INFO, Source.FOREST, "Запуск сервера леса");

            app.UseHttpsRedirection();
            botStatisticsSynchronizer.Start();
            routeRecordsSynchronizerService.Start();



            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{Id?}");
            });
        }
    }
}
