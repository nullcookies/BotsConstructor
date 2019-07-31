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


        //TokenReplacement 

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


        public async Task<IActionResult>StopBot(int botId)
        {
            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return StatusCode(403);
            }

            BotDB bot = context.Bots.Find(botId);

            if (bot != null)
            {
                if (bot.OwnerId == accountId)
                {
                    RouteRecord record = context.RouteRecords.Find(bot.Id);
                    if (record != null)
                    {
                        string forestLink = record.ForestLink;

                        try
                        {
                            //запрос на остановку бота
                            string result  = await Stub.SendPost("https://" + forestLink + "/StopBot?botId=" + bot.Id, "ostanovites");

                            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                            Console.WriteLine(result);
                            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

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
                    return Ok();
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
        }




        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RunBotForSalesFromDraft(int botId)
        {

            string forestUrl = "http://localhost:8080/Home/RunNewBot";

            string data = "botId=" + botId;

			try
			{
				var test63286 = Stub.SendPost(forestUrl, data).Result;
				return Ok();
			}
			catch(Exception ex)
			{
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