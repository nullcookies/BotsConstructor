using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using DataLayer.Models;
using Website.Other;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Website.Other.Filters;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Website.Services;
using DataLayer.Services;
using System.Linq;

namespace Website.Controllers
{
    public class BotForSalesSettingsController : Controller
    {

        ApplicationContext context;
        IHostingEnvironment _appEnvironment;
        StupidLogger logger;

        public BotForSalesSettingsController(ApplicationContext context, IHostingEnvironment appEnvironment, StupidLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
            this.logger = logger;
        }


        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Settings(int botId)
        {
            RouteRecord rr = context.RouteRecords.Find(botId);
            string forestLink = "localhost:8080/Forest";

            if (rr == null)
            {
                context.RouteRecords.Add(new RouteRecord() { BotId = botId, ForestLink = forestLink });
            }
            else
            {
                rr.ForestLink = forestLink;
            }            

            context.SaveChanges();

            BotDB bot = context.Bots.Find(botId);

            ViewData["botId"] = botId;
            ViewData["botType"] = bot.BotType;

            if (bot.Token != null)
            {
                //установить botUsername
            }
            
            RouteRecord record = context.RouteRecords.Find(botId);
            if (record != null)
            {
                //если работает, то вставить ссылку на лес для установки websocket-а
                ViewData["linkToForest"] = record.ForestLink;
            }

            //вставить статистику
            ViewData["ordersCount"] = bot.NumberOfOrders;
            ViewData["usersCount"] = bot.NumberOfUniqueUsers;
            ViewData["messagesCount"] = bot.NumberOfUniqueMessages;

            return View();
        }

        [HttpGet]
        public async Task MyWebsocket()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await SendMessage(webSocket, "beliberda");
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
        public async Task<IActionResult>StopBot(int botId)
        {
            Console.WriteLine("StopBot");


            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return StatusCode(403);
            }

            Console.WriteLine("accountId");
            BotDB bot = context.Bots.Find(botId);

            if (bot != null)
            {
                Console.WriteLine("bot!=null");
                if (bot.OwnerId == accountId)
                {
                    bot.BotUsername = "ping_uin_bot";
                    context.SaveChanges();
                    Console.WriteLine("bot.OwnerId == accountId");

                    RouteRecord record = context.RouteRecords.Find(bot.Id);
                    

                    if (record != null)
                    {
                        Console.WriteLine(" record != null) ");

                        //string forestLink = record.ForestLink;
                        string forestLink = "localhost:8080/Home"; ;
                        Console.WriteLine("forestLink = "+ forestLink);

                        try
                        {
                            Console.WriteLine("try");

                            //запрос на остановку бота
                            
                            string forestUrl = "http://localhost:8080/Home/StopBot";



                            Console.WriteLine(forestUrl);
                            string data = "botId=" + bot.Id;
                            Console.WriteLine("data="+data);
                            var test63286 = Stub.SendPost(forestUrl, data).Result;

                            Console.WriteLine(" await Stub.SendPost(" + forestLink );


                            //эта хрень говорит, что запись есть, но её на самом деле нет
                            //оно, похоже, использует какой-то кэш
                            //RouteRecord rr = context.RouteRecords.Find(botId);

                            RouteRecord normal_rr = context.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();

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
                                logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота botId={botId}," +
                                    $" accountId={accountId}. Лес ответил Ok, но не удалил RouteRecord из БД ");

                                return StatusCode(500);
                            }

                        }catch(Exception exe)
                        {
                            logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) не удалось выполнить post запрос на лес." +
                            $"Exception message ={exe.Message}");

                            return StatusCode(500);
                        }
                    }
                    else
                    {
                        logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"При остановке бота(botId={bot.Id}, " +
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
            BotDB bot = context.Bots.Find(botId);
            if (bot.Token == null)
            {
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                Console.WriteLine("Установите токен");
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                return StatusCode(510);
            }

            string forestUrl = "http://localhost:8080/Home/RunNewBot";

            string data = "botId=" + botId;

			try
			{
				var test63286 = Stub.SendPost(forestUrl, data).Result;
                
				return Ok();
			}
			catch(Exception ex)
			{
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
				return StatusCode(403, ex.Message);
			}
        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult DeleteBot(int botId)
        {
            try
            {
                var removableBot = context.Bots.Find(botId);
                context.Remove(removableBot);
                context.SaveChanges();
                return Ok();
            }
            catch(Exception ex)
            {
                logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Не удаётся удалить бота botId={botId}", ex);
                return StatusCode(500);
            }
        }









    }
}