using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using website.Models;
using System.Collections.Generic;
using website.Other;
using Markup;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using website.Other.Filters;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using website.Services;

namespace website.Controllers
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
            string botType = context.Bots.Find(botId).BotType;

            ViewData["botId"] = botId;
            ViewData["botType"] = botType;

            ViewData["botStatus"] = "working";
            ViewData["ordersCount"] = 44;
            ViewData["usersCount"] = 26;
            ViewData["messagesCount"] = 307;

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