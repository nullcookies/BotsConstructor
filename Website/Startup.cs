using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;
using Website.Services;
using DataLayer.Services;

namespace website
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization();


            string connection;

            bool isWindows =RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                connection = Configuration.GetConnectionString("PostgresConnectionDevelopment");
            }
            else
            {
                connection = Configuration.GetConnectionString("PostgresConnectionLinux");
            }

                                               
                             
            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationContext>(opt=>opt.UseNpgsql(connection)).BuildServiceProvider(); 
            

            //my
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("OnlyForAdmins", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, "admin");
                });
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddTransient<EmailMessageSender>();

            services.AddSingleton<StupidLogger>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                  options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
              });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (!isWindows)
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }


            app.UseStaticFiles();
            app.UseCookiePolicy();

            //my
            app.UseAuthentication();
            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
                
            };
            
            app.UseWebSockets(wsOptions);


         


            app.Use((context, next) =>
            {
                
                var userLangs = context.Request.Headers["Accept-Language"].ToString();
                var firstLang = userLangs.Split(',').FirstOrDefault();
                
                string lang = "";
                switch (firstLang)
                {
                    case "ru":
                        lang = "ru"; 
                        break;
                    default:
                        lang = "en"; 
                        break;
                }

                //switch culture
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                //save for later use
                context.Items["ClientLang"] = lang;
                context.Items["ClientCulture"] = Thread.CurrentThread.CurrentUICulture.Name;

                // Call the next delegate/middleware in the pipeline
                return next();
            });



            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Main}/{action=Index}/{id?}");
            });

           
            //Stub.RegisterInMonitor();
                      
          
        }


       

    }
}


////Политика настраивается в Startup
//[Authorize(Policy = "OnlyForAdmins")]
//public IActionResult IndexAdmin()
//{
//    return Content("Проверка пройдена");
//}