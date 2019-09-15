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
using Telegram.Bot;
namespace Website.Controllers
{
    public class TokenReplacementController : Controller
    {


        ApplicationContext context;
        IHostingEnvironment _appEnvironment;
        StupidLogger _logger;

        public TokenReplacementController(ApplicationContext context, IHostingEnvironment appEnvironment, StupidLogger  logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
            this._logger = logger;
        }



        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult TokenChange(int botId)
        {
            string currentToken = context.Bots.Find(botId).Token;
            ViewData["botId"] = botId;
            ViewData["currentToken"] = currentToken;
            return View();
        }
        


        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult TokenChange(int botId, string token)
        {
            if (token?.Length == 45)
            {
                var bot = context.Bots.Find(botId);
                try
                {
                    bot.BotName = new TelegramBotClient(token).GetMeAsync().Result.Username;                
                    bot.Token = token;
                    context.SaveChanges();
                    ViewData["currentToken"] = token;
                }catch(Exception ee)
                {
                    int? accountId = Stub.GetAccountIdFromCookies(HttpContext);
                    _logger.Log(LogLevelMyDich.USER_ERROR, Source.WEBSITE, $"Сайт. Установка токена для бота " +
                        $"botId={botId}, accountId= {accountId}, token = {token}. Не удалось " +
                        $"узнать username бота.  Возможно, такой токен не существует. exception="+
                       ee.Message);

                    ModelState.AddModelError("", "Ошибка обработки токена.");
                    string currentToken = context.Bots.Find(botId).Token;
                    ViewData["currentToken"] = currentToken;
                }
            }
            else
            {
                ModelState.AddModelError("", "Неверная длина токена");
                _logger.Log(LogLevelMyDich.USER_ERROR, Source.WEBSITE, $"Введён токен неверной длины token={token}");
                string currentToken = context.Bots.Find(botId).Token;
                ViewData["currentToken"] = currentToken;
            }


            ViewData["botId"] = botId;

            return View();
        }
    }
}