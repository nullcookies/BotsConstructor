using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Monitor.TelegramAgent;

namespace Monitor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MyTelegramAgent>();
            services.AddSingleton<TelegramAgentHelperBot>();
            
            
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, TelegramAgentHelperBot bot)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=NewsCreator}/{action=Index}/{id?}");
            });
            
            
        }
    }
}