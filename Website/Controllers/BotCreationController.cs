
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

            int accountId = (int)HttpContext.Items["accountId"];

            try
            {
                string token = tokenModel?.Token;
                string botUsername = new TelegramBotClient(token).GetMeAsync().Result.Username;

                //Создание нового бота для продаж с пустой разметкой
                BotDB bot = new BotDB() { OwnerId = accountId, BotType = "BotForSales", Token = token, BotName = botUsername };

                _contextDb.Bots.Add(bot);

                //Создание статистики для бота
                BotForSalesStatistics botForSalesStatistics = new BotForSalesStatistics() { Bot = bot, NumberOfOrders = 0, NumberOfUniqueMessages = 0, NumberOfUniqueUsers = 0 };

                _contextDb.BotForSalesStatistics.Add(botForSalesStatistics);

				var statusGroup = new OrderStatusGroup()
				{
					Name = "Стандартный набор статусов",
					OwnerId = accountId,
					OrderStatuses = new List<OrderStatus>()
					{
						new OrderStatus() {Name = "В обработке", Message = "Ваш заказ находится в обработке."},
						new OrderStatus() {Name = "В пути", Message = "Ваш заказ в пути."},
						new OrderStatus() {Name = "Принят", Message = "Ваш заказ был принят."},
						new OrderStatus() {Name = "Отменён", Message = "Ваш заказ был отменён."}
					}
				};

				_contextDb.OrderStatusGroups.Add(statusGroup);

                _contextDb.SaveChanges();
                
				return RedirectToAction("SalesTreeEditor", "BotForSalesEditing", new { bot.Id });

            }
            catch (Exception ee)
            {
                _logger.Log(LogLevelMyDich.USER_ERROR, Source.WEBSITE, $"Сайт. Создание нового бота. При " +
                    $"запросе botUsername было выброшено исключение (возможно, введённый" +
                    $"токен был специально испорчен)"+ee.Message, accountId:accountId);

                ModelState.AddModelError("", "Ошибка обработки токена.");
            }
           

            return View("BotForSalesTokenEntry");
        }
    }
}
