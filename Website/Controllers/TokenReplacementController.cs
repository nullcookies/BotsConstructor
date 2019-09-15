﻿using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using Telegram.Bot;
using Website.Other;
using Website.Other.Filters;


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
            
            var bot = context.Bots.Find(botId);
            try
            {
                bot.BotName = new TelegramBotClient(token).GetMeAsync().Result.Username;                
                bot.Token = token;
                context.SaveChanges();
            }catch(Exception ee)
            {
                int? accountId = Stub.GetAccountIdFromCookies(HttpContext);

                _logger.Log(LogLevelMyDich.USER_ERROR, Source.WEBSITE, $"Сайт. Установка токена для бота " +
                    $"botId={botId}, accountId= {accountId}, token = {token}. Не удалось " +
                    $"узнать username бота.  Возможно, такой токен не существует. ", ex: ee);

                ModelState.AddModelError("", "Ошибка обработки токена.");
            }
          
            ViewData["currentToken"] = bot.Token;
            ViewData["botId"] = botId;

            return View();
        }
    }
}