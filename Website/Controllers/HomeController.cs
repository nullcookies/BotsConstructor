﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Other;

namespace Website.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ApplicationContext context;

        public HomeController(ApplicationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

        public IActionResult Index()
        {
            return RedirectToAction("MyBots");
        }

        public IActionResult MyBots()
        {
            int accountId = 0;
            try{
                accountId =  Stub.GetAccountIdFromCookies(HttpContext)  ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return StatusCode(403);
            }
            
            var bots = context.Bots.Where(_bot => _bot.OwnerId == accountId);
            List<BotOnHomePage> modelBots = new List<BotOnHomePage>();
			int i = 1;
			foreach (var bot in bots)
			{
                
                string name = "";
                if (bot.Token == null)
                {
                    name = "Бот ещё не запускался";
                }
                else
                {
                    //TODO запрос имени бота
                    name = "botUsername";
                }
				modelBots.Add(new BotOnHomePage() { Number = i++, Name = name, BotId = bot.Id, Status = "Работает" });
			}
            ViewData["bots"] = modelBots;
            
            
            return View();
        }

		[HttpGet]
        public IActionResult BotCreation()
        {
            return View();
        }

        //Автоматически вызывается при ошибке 
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
