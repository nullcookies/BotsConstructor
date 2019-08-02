using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using System.Collections.Generic;
using Website.Other;

using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Website.Other.Filters;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using DataLayer.Models;

namespace Website.Controllers
{
    [Authorize]
    public class BotEditingController : Controller
    {
        ApplicationContext context;
        IHostingEnvironment _appEnvironment;

        public BotEditingController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
        }


        public IActionResult EditBot(int botId)
        {
            string botType = context.Bots.Find(botId)?.BotType;
            switch (botType)
            {
                case "BotForSales":
                    return RedirectToAction("BotForSales", "BotForSalesEditing", new { botId });
                default:
                    throw new Exception("Неизвестный тип бота");
            }
        }


        public IActionResult CreateNewBotForSales()
        {

            int accountId = 0;
            try
            {
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }
            catch
            {
                return RedirectToAction("Account", "Login");
            }


            //Создание нового бота для продаж с пустой разметкой
            BotDB bot = new BotDB() { BotName = "New bot", OwnerId = accountId, BotType = "BotForSales" };
            context.Bots.Add(bot);

            //Создание статистики для бота
            BotForSalesStatistics botForSalesStatistics = new BotForSalesStatistics()
            {
                BotId = bot.Id
            };

            context.BotForSalesStatistics.Add(botForSalesStatistics);

            context.SaveChanges();

            int botId = bot.Id;

            return RedirectToAction("BotForSales", "BotForSalesEditing", new { botId });
        }





    }
}
