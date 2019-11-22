using DataLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using MyLibrary;
using Website.Other.Middlewares;
using Website.Services;

namespace Website
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _environment = environment;
        }

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



            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(DbContextFactory.GetConnectionString()))
                .BuildServiceProvider();


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

            services.AddSingleton<SimpleLogger>();
            services.AddSingleton<OrdersCountNotificationService>();
            services.AddSingleton<BotForSalesStatisticsService>();
            services.AddSingleton<TotalLog>();
            services.AddSingleton<BotsAirstripService>();
          

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.LoginPath = new PathString("/SignIn/Login");
                  options.AccessDeniedPath = new PathString("/SignIn/Login");
              });
        }


        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ApplicationContext _contextDb,
            TotalLog totalLog,
            SimpleLogger logger)
        {

            logger.Log(LogLevel.IMPORTANT_INFO,
                Source.WEBSITE,
                "Запуск сервера сайта");
            
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");
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
                KeepAliveInterval = TimeSpan.FromSeconds(1),
                ReceiveBufferSize = 4 * 1024

            };

            app.UseWebSockets(wsOptions);


            //сохранение в удобном виде для настроек локализации
            app.Use((context, next) =>
            {

                var userLanguages = context.Request.Headers["Accept-Language"].ToString();
                var firstLang = userLanguages.Split(',').FirstOrDefault();

                string lang = "";
                if (firstLang != null && firstLang.ToLower().Contains("ru"))
                    lang = "ru";
                else
                    lang = "en";


                //switch culture
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                //save for later use
                context.Items["ClientLang"] = lang;
                context.Items["ClientCulture"] = Thread.CurrentThread.CurrentUICulture.Name;

                // Call the next delegate/middleware in the pipeline
                return next();
            });


            //запись accountId если удалось найти в куки
            app.Use((context, next) =>
            {
                string idStr = context.User.FindFirst(x => x.Type == "userId")?.Value;

                if (int.TryParse(idStr, out int id))
                {
                    context.Items["accountId"] = id;
                }

                return next();
            });


            
            app.Use(async (context, next) =>
            {
                totalLog.Log(context);
                await next.Invoke();
            });



            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Main}/{action=Index}/{id?}");
            });

            
            
        }
    }
}

