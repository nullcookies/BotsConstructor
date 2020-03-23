using DataLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monitor.Services;
using MyLibrary;

namespace Monitor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSingleton<SimpleLogger>();
            services.AddSingleton<DiagnosticService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(DbConnectionGlobals.GetConnectionString()))
                .BuildServiceProvider();
            
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/SignIn/Login");
                    options.AccessDeniedPath = new PathString("/SignIn/Login");
                });
            
        }
        
        public void Configure(
                IApplicationBuilder app,
                IHostingEnvironment env,
                DiagnosticService diagnosticService, 
                SimpleLogger logger)
        {
            logger.Log(LogLevel.IMPORTANT_INFO, Source.MONITOR,"Старт монитора");

            diagnosticService.StartPingAsync();
//            app.UseHttpsRedirection();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            
            //my
            app.UseAuthentication();
            
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}