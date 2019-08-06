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
using Website.Services;
using DataLayer.Services;
using DataLayer.Models;
using System.Diagnostics;

namespace Website.Controllers
{
    public class BotForSalesSettingsController : Controller
    {

        ApplicationContext _contextDb;
        IHostingEnvironment _appEnvironment;
        StupidLogger _logger;
        BotForSalesStatisticsService _botForSalesStatisticsService;

        public BotForSalesSettingsController(ApplicationContext context, 
                IHostingEnvironment appEnvironment, 
                StupidLogger _logger, 
                BotForSalesStatisticsService botForSalesStatisticsService)
        {
            this._contextDb = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
            this._logger = _logger;
            _botForSalesStatisticsService = botForSalesStatisticsService;
        }


        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Settings(int botId)
        {

            BotDB bot = _contextDb.Bots.Find(botId);
            
            ViewData["botId"] = botId;
            ViewData["botType"] = bot.BotType;
            ViewData["usersCount"] = 0;
            ViewData["ordersCount"] = 0;
            ViewData["messagesCount"] = 0;


            return View();
        }
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetBotForSalesStatistics(int botId)
        {
            _logger.Log(LogLevelMyDich.INFO, ErrorSource.WEBSITE, "Сайт. Опрос стастистики бота через " +
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
                _logger.Log(LogLevelMyDich.ERROR, ErrorSource.WEBSITE, "Сайт. Опрос стастистики бота через " +
                    "ajax (на клиенте не доступен webSocket или кто-то балуется). Не " +
                    "удаётся отправить статистику бота. Ошибка "+ee.Message);

                return StatusCode(500);
            }

        }

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

                _botForSalesStatisticsService.RegisterInNotificationSystem(botId, webSocket);

                await socketFinishedTcs.Task;


            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
        private async Task SendMessage(WebSocket webSocket, string message)
        {
            
            //working
            //fail
            //stopped
            string botStatus = "working";
            int ordersCount = 45;
            int usersCount = 26;
            int messagesCount = 308;

            JObject JObj = new JObject
            {
                { "botStatus", botStatus},
                { "ordersCount", ordersCount},
                { "usersCount", usersCount},
                { "messagesCount", messagesCount},
            };

            string jsonString = JsonConvert.SerializeObject(JObj);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var arraySegment = new ArraySegment<byte>(bytes);
            await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

            Thread.Sleep(2000);



        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult StopBot(int botId)
        {
           
            _logger.Log(LogLevelMyDich.INFO, ErrorSource.WEBSITE, "Остановка бота");

            //TODO Повторное извлечение accountId из cookies
            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Из cookies не удалось извлечь accountId");
            }catch{
                return StatusCode(403);
            }


            JObject jObject = null;


            BotDB bot = _contextDb.Bots.Find(botId);

            if (bot != null)
            {
                if (bot.OwnerId == accountId)
                {
                    
                    RouteRecord record = _contextDb.RouteRecords.Find(bot.Id);                    

                    if (record != null)
                    {
                     
                        try
                        {

                            //запрос на остановку бота

                            string forestUrl = record.ForestLink + "/Home/StopBot";
                            

                            Console.WriteLine(forestUrl);
                            string data = "botId=" + bot.Id;
                            Console.WriteLine("data="+data);
                            var test63286 = Stub.SendPost(forestUrl, data).Result;

                            Console.WriteLine(" await Stub.SendPost(" + forestUrl);

                            RouteRecord normal_rr = _contextDb.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();

                            Console.WriteLine("uteRecord rr = context.RouteRecords.Find(b");

                            if (normal_rr == null)
                            {
                                //лес нормально удалил запись о боте
                                Console.WriteLine("//лес нормально удалил запись о боте");
                                 jObject = new JObject()
                                    {
                                        { "success", true}
                                    };
                                return Json(jObject);
                            }
                            else
                            {
                                Console.WriteLine("//лес не нормально удалил запись о боте");
                                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"При остановке бота botId={botId}," +
                                    $" accountId={accountId}. Лес ответил Ok, но не удалил RouteRecord из БД ");

                                jObject = new JObject()
                                    {
                                        { "success", false},
                                        {"failMessage", " Не удалось остановить бота." }
                                    };
                                return Json(jObject);
                            }

                        }catch(Exception exe)
                        {
                            _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) не удалось выполнить post запрос на лес." +
                            $"Exception message ={exe.Message}");

                            jObject = new JObject()
                                    {
                                        { "success", false},
                                        {"failMessage", " Не удалось остановить бота. Возможно, есть проблемы с соединением." }
                                    };
                            return Json(jObject);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) в БД не была найдена" +
                            $"запись о сервере на котором бот работает. Возможно, она была удалена или не добавлена.");

                        jObject = new JObject()
                            {
                                { "success", false},
                                {"failMessage", " Бот уже остановлен." }
                            };
                        return Json(jObject);

                    }
                    
                }
                else
                {
                    jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", " У вас не доступа к этому боту." }
                        };
                    return Json(jObject);
                }
            }
            else
            {
                jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", " Такого бота не существует." }
                        };
                return Json(jObject);
            }

            
        }




        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RunBotForSalesFromDraft(int botId)
        {
            _logger.Log(LogLevelMyDich.INFO, ErrorSource.WEBSITE, $"Сайт. Запуск бота. botId={botId}");
            
            BotDB bot = _contextDb.Bots.Find(botId);
            JObject jObject = null;

            //TODO бот оплачен?

            //без токена запускаться нельзя
            if (bot.Token == null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, ErrorSource.WEBSITE, $"Попытка запутить бота без токена. botId={botId}");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Запуск бота не возможен без токена. Установите токен в настройках бота." }
                };
                return Json(jObject);
            }

            //без разметки запускаться нельзя
            if (bot.Markup == null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, ErrorSource.WEBSITE, $"Попытка запутить бота без разметки. botId={botId}");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Запуск бота не возможен без разметки. Нажмите \"Редактировать разметку черновика\"" }
                };
                return Json(jObject);
            }

            //Если бот уже запущен, вернуть ошибку
            RouteRecord existingRecord = _contextDb.RouteRecords.Find(botId);
            if (existingRecord != null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, ErrorSource.WEBSITE, $"Попытка запутить запущенного бота.");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Этот бот уже запущен. (" }
                };
                return Json(jObject);
            }



			try
			{
                //TODO брать ссылку из монитора
                string forestUrl = "http://localhost:8080/Home/RunNewBot";

                //Попытка запуска в лесу
                string data = "botId=" + botId;
				var result = Stub.SendPost(forestUrl, data).Result;

                JObject answer = (JObject) JsonConvert.DeserializeObject(result);

                bool successfulStart = (bool) answer["success"];
                string failMessage = (string) answer["failMessage"];

                if (successfulStart)
                {
                    //Лес нормально сделал запись о запуске?
                    RouteRecord rr = _contextDb.RouteRecords.Find(botId);
                    if (rr != null)
                    {
                        //лес нормально сделал запись о том, что бот запущен

                        jObject = new JObject()
                        {
                            { "success", true}
                        };
                        return Json(jObject);
                    }
                    else
                    {
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"Лес вернул Ок (нормальный запуск бота), но не сделал запись в бд. botId={botId}");

                        jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Ошибка сервера при запуске бота" }
                        };
                        return Json(jObject);

                    }
                }
                else
                {
                    jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Ошибка сервера при запуске бота."+failMessage }
                        };
                    return Json(jObject);
                }               

			}
			catch(Exception ex)
			{
                                
                _logger.Log(LogLevelMyDich.ERROR, ErrorSource.WEBSITE, $"Не удалось запустить бота. botId={botId}. ex.Message={ex.Message}");

                jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Не удалось запустить бота. Возможно, возникла проблема соединения" }
                        };
                return Json(jObject);
            }
        }

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
                        _logger.Log(LogLevelMyDich.I_AM_AN_IDIOT, ErrorSource.WEBSITE, $"Не удалось остановить бота botId={botId}");
                        return StatusCode(500);
                    }
                }

                _contextDb.Remove(removableBot);
                _contextDb.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"Не удаётся удалить бота botId={botId}", ex);
                return StatusCode(500);
            }
        }









    }
}