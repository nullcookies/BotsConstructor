using System.Runtime.InteropServices;
using DataLayer;
using DataLayer.Models;
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
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(DbContextFactory.GetConnectionString(_environment)))
                .BuildServiceProvider();

			services.AddSingleton<DbConnector>();
            services.AddSingleton<StupidLogger>();
            services.AddSingleton<BotStatisticsSynchronizer>();



        }

        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            BotStatisticsSynchronizer botStatisticsSynchronizer,
            StupidLogger logger)
        {

            logger.Log(LogLevelMyDich.IMPORTANT_INFO,
                Source.WEBSITE,
                "Запуск сервера леса");


            app.UseDeveloperExceptionPage();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
