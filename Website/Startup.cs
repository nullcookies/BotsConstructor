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
using Website.Other.Middlewares;

namespace Website
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

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                connection = Configuration.GetConnectionString("PostgresConnectionDevelopment");
            }
            else
            {
                connection = Configuration.GetConnectionString("PostgresConnectionLinux");
            }

            if (connection == null)
            {
                throw  new Exception("Не удалось открыть конфиг файл");
            }

            Console.WriteLine("строка="+connection);

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationContext>(opt => opt.UseNpgsql(connection))
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
            //services.AddTransient<StupidBotForSalesBookkeeper>();

            services.AddSingleton<StupidLogger>();
            services.AddSingleton<OrdersCountNotificationService>();
            services.AddSingleton<BotForSalesStatisticsService>();
            services.AddSingleton<TotalLog>();
            services.AddSingleton<BotsAirstripService>();

            //services.AddTransient<MoneyCollectorService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/SignIn/Login");
                  options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/SignIn/Login");
              });
        }


        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ApplicationContext _contextDb,
            TotalLog totalLog,
            //MoneyCollectorService moneyCollectorService,
            StupidLogger logger)
        {

            logger.Log(LogLevelMyDich.IMPORTANT_INFO,
                Source.WEBSITE,
                "Запуск сервера сайта");
            //оно не хочет очищать таблицу
            //_contextDb.Database.ExecuteSqlCommand("TRUNCATE TABLE [RouteRecords]");

            _contextDb.RouteRecords.RemoveRange(_contextDb.RouteRecords);
            _contextDb.SaveChanges();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");
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
                KeepAliveInterval = TimeSpan.FromSeconds(1),
                ReceiveBufferSize = 4 * 1024

            };

            app.UseWebSockets(wsOptions);


            //сохранение в удобном виде для настроек локализации
            app.Use((context, next) =>
            {

                var userLangs = context.Request.Headers["Accept-Language"].ToString();
                var firstLang = userLangs.Split(',').FirstOrDefault();

                string lang = "";
                if (firstLang.ToLower().Contains("ru"))
                {
                    lang = "ru";
                }
                else
                {
                    lang = "en";
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



            //как это засунуть в Middleware?
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


////Политика настраивается в Startup
//[Authorize(Policy = "OnlyForAdmins")]
//public IActionResult IndexAdmin()
//{
//    return Content("Проверка пройдена");
//}


