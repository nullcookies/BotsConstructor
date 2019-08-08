using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Website.Other;
using Website.Other.Filters;
using Website.Services;
using Website.Services.Bookkeeper;

namespace Website.Controllers
{
    public class BotForSalesSettingsController : Controller
    {

        StupidLogger _logger;
        ApplicationContext _contextDb;
        IHostingEnvironment _appEnvironment;
        BotForSalesStatisticsService _botForSalesStatisticsService;
        StupidBotForSalesBookkeeper _bookkeper;

        public BotForSalesSettingsController(ApplicationContext context, 
                IHostingEnvironment appEnvironment, 
                StupidLogger _logger, 
                BotForSalesStatisticsService botForSalesStatisticsService,
                StupidBotForSalesBookkeeper _bookkeper)
        {
            this._logger = _logger;
            _appEnvironment = appEnvironment;
            this._contextDb = context ?? throw new ArgumentNullException(nameof(context));
            _botForSalesStatisticsService = botForSalesStatisticsService;
            this._bookkeper = _bookkeper;
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
                            string data = "botId=" + bot.Id;
                            var result = Stub.SendPost(forestUrl, data).Result;

                            RouteRecord normal_rr = _contextDb.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();                            

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
                                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота botId={botId}," +
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
                            _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
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
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
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
            
            BotDB bot = _contextDb.Bots.Find(botId);
            JObject jObject = null;

            //TODO бот оплачен?

            //без токена запускаться нельзя
            if (bot.Token == null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить бота без токена. botId={botId}");
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
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить бота без разметки. botId={botId}");
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
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить запущенного бота.");
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
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"Лес вернул Ок (нормальный запуск бота), но не сделал запись в бд. botId={botId}");

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
                                
                _logger.Log(LogLevelMyDich.ERROR, Source.WEBSITE, $"Не удалось запустить бота. botId={botId}. ex.Message={ex.Message}");

                jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Не удалось запустить бота. Возможно, возникла проблема соединения" }
                        };
                return Json(jObject);
            }
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

        [HttpGet]
        public IActionResult PriceDetails(int botId)
        {
            StupidPriceInfo _pi = _bookkeper.GetPriceInfo(botId);

            ViewData["sum"] = _pi.SumToday;
            ViewData["dailyPrice"] = _pi.DailyConst;
            ViewData["orderPrice"] = _pi.OneAnswerPrice;
            ViewData["countOfOrders"] = _pi.AnswersCountToday;
            ViewData["number_of_orders_over_the_past_week"] = _pi.Number_of_orders_over_the_past_week;


            return View();
        }
    }
}