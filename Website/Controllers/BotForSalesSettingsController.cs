﻿using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Website.Other;
using Website.Other.Filters;
using Website.Services;

//Дичь

namespace Website.Controllers
{
    [Authorize]
    public class BotForSalesSettingsController : Controller
    {
        readonly StupidLogger _logger;
        IHostingEnvironment _appEnvironment;
        readonly ApplicationContext _contextDb;
        readonly BotsAirstripService _botsAirstripService;
        readonly BotForSalesStatisticsService _botForSalesStatisticsService;

        public BotForSalesSettingsController(ApplicationContext context, 
                IHostingEnvironment appEnvironment, 
                StupidLogger logger, 
                BotForSalesStatisticsService botForSalesStatisticsService,
                BotsAirstripService botsAirstripService)
        {
            _logger = logger;
            _appEnvironment = appEnvironment;
            _botsAirstripService = botsAirstripService;
            _botForSalesStatisticsService = botForSalesStatisticsService;
            _contextDb = context ?? throw new ArgumentNullException(nameof(context));
        }


        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Settings(int botId)
        {
            BotDB bot = _contextDb.Bots.Find(botId);

            ViewData["sum"] = 0;

            ViewData["botId"] = botId;
            ViewData["botType"] = bot.BotType;
            ViewData["usersCount"] = 0;
            ViewData["ordersCount"] = 0;
            ViewData["messagesCount"] = 0;

            return View();
        }

        /// <summary>
        /// Запрос стастистики для бота через ajax
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetBotForSalesStatistics(int botId)
        {
            _logger.Log(LogLevelMyDich.INFO, Source.WEBSITE, "Сайт. Опрос стастистики бота через " +
                    "ajax (на клиенте не доступен webSocket или кто-то балуется).");
            try
            {
                bool botWorks = _contextDb.RouteRecords.Find(botId) != null? true:false;
                BotForSalesStatistics stat = _contextDb.BotForSalesStatistics.Find(botId);
                JObject jObj = new JObject
                    {
                        { "botWorks",       botWorks},
                        { "ordersCount",    stat.NumberOfOrders},
                        { "usersCount",     stat.NumberOfUniqueUsers},
                        { "messagesCount",  stat.NumberOfUniqueMessages},
                    };
                return Json(jObj);

            }catch(Exception ee)
            {
                _logger.Log(LogLevelMyDich.ERROR, Source.WEBSITE, "Сайт. Опрос стастистики бота через " +
                    "ajax (на клиенте не доступен webSocket или кто-то балуется). Не " +
                    "удаётся отправить статистику бота. Ошибка "+ee.Message);

                return StatusCode(500);
            }

        }

        /// <summary>
        /// Установка websocket-a для подписки на обновление статистики бота 
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public async Task MyWebsocket(int botId)
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var socketFinishedTcs = new TaskCompletionSource<object>();

                //Регистрация 
                _botForSalesStatisticsService.RegisterInNotificationSystem(botId, webSocket);

                //Магия
                await socketFinishedTcs.Task;
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }


        /// <summary>
        /// Запрос на остановку бота черех ajax
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult StopBot(int botId)
        {           
            _logger.Log(LogLevelMyDich.INFO, Source.WEBSITE, "Остановка бота");

            int accountId = (int)HttpContext.Items["accountId"];

            JObject answer = _botsAirstripService.StopBot(botId, accountId);

            return Json(answer);
            
        }



        /// <summary>
        /// Запрос на запуск бота через ajax
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RunBotForSalesFromDraft(int botId)
        {
            _logger.Log(LogLevelMyDich.INFO, Source.WEBSITE, $"Сайт. Запуск бота. botId={botId}");

            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Из cookies не удалось извлечь accountId");
            }catch{
                return StatusCode(403);
            }

            JObject answer = _botsAirstripService.StartBot(botId, accountId);
            return Json(answer);
        }


        //Запрос на удаление бота через ajax
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult DeleteBot(int botId)
        {
            try
            {
                var removableBot = _contextDb.Bots.Find(botId);

                RouteRecord rr = _contextDb.RouteRecords.Find(botId);

                //Удаляемый бот запущен
                if (rr != null)
                {
                    //Остановка
                    IActionResult res = StopBot(botId);
                    if (res != Ok())
                    {
                        _logger.Log(LogLevelMyDich.I_AM_AN_IDIOT, Source.WEBSITE, $"Не удалось остановить бота botId={botId}");
                        return StatusCode(500);
                    }
                }

                //снять со счёта стоимость бота
                //StupidPriceInfo priceInfo = _bookkeper.GetPriceInfo(botId, DateTime.UtcNow);
                Account account = _contextDb.Accounts.Find(removableBot.OwnerId);

                ////Цена адекватная
                //if (priceInfo.SumToday >= 0)
                //{
                //    account.Money -= priceInfo.SumToday;
                //}
                //else
                //{
                //    _logger.Log(LogLevelMyDich.FATAL,
                //        Source.WEBSITE,
                //        $"Удаление бота. При снятии денег с аккаунта {removableBot.OwnerId} цена была отрицательной");
                //    throw new Exception("");
                //}

                _contextDb.SaveChanges();

                _contextDb.Remove(removableBot);
                _contextDb.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"Не удаётся удалить бота botId={botId}", ex:ex);
                return StatusCode(500);
            }
        }

    }
}