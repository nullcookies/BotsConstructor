using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class MyBotsController : Controller
    {
        private readonly ApplicationContext _contextDb;
        private readonly SimpleLogger _logger;

        public MyBotsController(ApplicationContext context, SimpleLogger logger)
        {
            _contextDb = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public IActionResult MyBots()
        {
            int accountId = (int) HttpContext.Items["accountId"];

            var accountBots = _contextDb.Bots.Where(_bot => _bot.OwnerId == accountId);
            var rrs = _contextDb.RouteRecords
                .Include(_rr => _rr.Bot)
                .Where(_rr => _rr.Bot.OwnerId == accountId);

            List<BotOnHomePage> modelBots = GetBotsModelView(accountBots, rrs);
            ViewData["bots"] = modelBots;

            return View();
        }


        private List<BotOnHomePage> GetBotsModelView(IQueryable<BotDB> bots, IQueryable<RouteRecord> rrs)
        {
            List<BotOnHomePage> modelBots = new List<BotOnHomePage>();

            foreach (var bot in bots)
            {
                int countOfRouteRecords = rrs.Count(_rr => _rr.BotId == bot.Id);
                string status = GetBotStatusByCountOfRouteRecords(countOfRouteRecords);
                modelBots.Add(new BotOnHomePage {Name = bot.BotName, BotId = bot.Id, Status = status});
            }

            return modelBots;
        }

        private string GetBotStatusByCountOfRouteRecords(int countOfRouteRecords)
        {
            switch (countOfRouteRecords)
            {
                case 0:
                    return "⛔️Остановлен⛔️";
                case 1:
                    return "✅Работает✅";
                default:
                    throw new Exception("Сайт. При " + "запросе всех RouteRecord из бд их количество для бота  " +
                                        "оказалоссь больше одной");
            }
        }
    }
}