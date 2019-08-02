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
            _logger.Log(LogLevelMyDich.INFO, "Сайт. Опрос стастистики бота через " +
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
                _logger.Log(LogLevelMyDich.ERROR, "Сайт. Опрос стастистики бота через " +
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
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n");
            Console.WriteLine("Остановка бота");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\\n");
            //TODO Повторное извлечение accountId из cookies
            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Из cookies не удалось извлечь accountId");
            }catch{
                return StatusCode(403);
            }

            BotDB bot = _contextDb.Bots.Find(botId);

            if (bot != null)
            {
                if (bot.OwnerId == accountId)
                {
                    Console.WriteLine("bot.OwnerId == accountId");

                    RouteRecord record = _contextDb.RouteRecords.Find(bot.Id);
                    

                    if (record != null)
                    {
                        Console.WriteLine(" record != null) ");

                        try
                        {
                            Console.WriteLine("try");

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
                                return Ok();
                            }
                            else
                            {
                                Console.WriteLine("//лес не нормально удалил запись о боте");
                                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота botId={botId}," +
                                    $" accountId={accountId}. Лес ответил Ok, но не удалил RouteRecord из БД ");

                                return StatusCode(500);
                            }

                        }catch(Exception exe)
                        {
                            _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) не удалось выполнить post запрос на лес." +
                            $"Exception message ={exe.Message}");

                            return StatusCode(500);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) в БД не была найдена" +
                            $"запись о сервере на котором бот работает. Возможно, она была удалена или не добавлена.");
                    }
                    
                }
                else
                {
                    return StatusCode(403);
                }
            }
            else
            {
                return StatusCode(404);
            }

            return StatusCode(418);
        }




        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RunBotForSalesFromDraft(int botId)
        {
            _logger.Log(LogLevelMyDich.INFO, $"Сайт. Запуск бота. botId={botId}");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            Console.WriteLine("Сайт. Запуск бота");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

            BotDB bot = _contextDb.Bots.Find(botId);

            if (bot.Token == null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, $"Попытка запутить бота без токена. botId={botId}");
                return StatusCode(500);
            }

            string forestUrl = "http://localhost:8080/Home/RunNewBot";

            string data = "botId=" + botId;

			try
			{
				var result = Stub.SendPost(forestUrl, data).Result;

                //проверка нормального запуска

                RouteRecord rr = _contextDb.RouteRecords.Find(botId);

                if (rr != null)
                {
                    //лес нормально записал запись о том, что бот запущен
				    return Ok();
                }
                else
                {
                    _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес вернул Ок (нормальный запуск бота), но не сделал запись в бд. botId={botId}");
                    return StatusCode(500);
                }

			}
			catch(Exception ex)
			{
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Не удалось запустить бота. botId={botId}. ex.Message={ex.Message}");


                return StatusCode(403, ex.Message);
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
                        _logger.Log(LogLevelMyDich.I_AM_AN_IDIOT, $"Не удалось остановить бота botId={botId}");
                        return StatusCode(500);
                    }
                }

                _contextDb.Remove(removableBot);
                _contextDb.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Не удаётся удалить бота botId={botId}", ex);
                return StatusCode(500);
            }
        }









    }
}