using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Other;

namespace Website.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ApplicationContext _contextDb;
        StupidLogger _logger;
        public HomeController(ApplicationContext context, StupidLogger logger)
        {
            this._contextDb = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;

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
            
            var bots = _contextDb.Bots.Where(_bot => _bot.OwnerId == accountId);
            var rrs = _contextDb.RouteRecords.ToList();
            List<BotOnHomePage> modelBots = new List<BotOnHomePage>();
			int i = 1;
			foreach (var bot in bots)
			{
                
                string name = "";
                if (bot.BotName == null)
                {
                    name = "Бот ещё не запускался";
                }
                else
                {
                    name = bot.BotName;
                }

                string status = "";
                int countOfRouteRecords = rrs.Where(_rr => _rr.BotId == bot.Id).Count();
                switch (countOfRouteRecords)
                {
                    case 0:
                        status = "⛔️Остановлен⛔️";
                        break;
                    case 1:
                        status = "✅Работает✅";
                        break;
                    default:
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, ErrorSource.WEBSITE, $"Сайт. При " +
                            $"запросе всех RouteRecord из бд их количество для бота {bot.Id} " +
                            $"оказалоссь больше одной");
                        return StatusCode(500);
                }
                modelBots.Add(new BotOnHomePage() { Number = i++, Name = name, BotId = bot.Id, Status = status });
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
