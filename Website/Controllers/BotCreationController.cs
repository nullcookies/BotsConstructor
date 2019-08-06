
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLayer.Models;
using Website.Other;
using Website.Services;
using Website.ViewModels;
using DataLayer.Services;
using Telegram.Bot;

namespace Website.Controllers
{
    public class BotCreationController : Controller
    {

        ApplicationContext _contextDb;
        StupidLogger _logger;

        public BotCreationController(ApplicationContext contextDb, StupidLogger logger)
        {
            _contextDb = contextDb;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("BotTypeSelection");
        }

        [HttpGet]
        public IActionResult BotForSalesTokenEntry()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult CreateNewBotForSales(TokenChange tokenModel)
        {

            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Не удалось извлечь accountId из cookies");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            string token = tokenModel?.Token;
            string botUsername = null;
            
            try
            {
                botUsername = new TelegramBotClient(token).GetMeAsync().Result.Username;

                //Создание нового бота для продаж с пустой разметкой
                BotDB bot = new BotDB() { OwnerId = accountId, BotType = "BotForSales", Token = token, BotName = botUsername };

                _contextDb.Bots.Add(bot);

                //Создание статистики для бота
                BotForSalesStatistics botForSalesStatistics = new BotForSalesStatistics()
                {
                    BotId = bot.Id
                };

                _contextDb.BotForSalesStatistics.Add(botForSalesStatistics);
                _contextDb.SaveChanges();

                int botId = bot.Id;

                return RedirectToAction("BotForSales", "BotForSalesEditing", new { botId });

            }
            catch (Exception ee)
            {
                Console.WriteLine("\n\n\n\n\n\n\n");
                Console.WriteLine(ee.Message);
                Console.WriteLine("\n\n\n\n\n\n\n");
                _logger.Log(LogLevelMyDich.USER_ERROR, ErrorSource.WEBSITE, $"Сайт. Создание нового бота. При " +
                    $"запросе botUsername было выброшено исключение (возможно, введённый" +
                    $"токен был специально испорчен)"+ee.Message);

                ModelState.AddModelError("", "Ошибка обработки токена.");
            }
           


            

            return View("BotForSalesTokenEntry");
        }
    }
}
