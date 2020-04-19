using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Other.Filters;
using Website.Services;

namespace Website.Controllers.Private_office.My_bots
{
    public class BotMassMailingController:Controller
    {
        private readonly BotMassMailingService _botMassMailingService;
        private readonly ApplicationContext _dbContext;

        public BotMassMailingController(BotMassMailingService botMassMailingService, ApplicationContext dbContext)
        {
            _botMassMailingService = botMassMailingService;
            _dbContext = dbContext;
        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Index(int botId)
        {
            ViewData["botId"] = botId;
            return View();
        }
        
        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public async Task<IActionResult> Index(int botId, BotMassMailingViewModel viewModel)
        {
            
            try
            {
                var botDb = _dbContext.Bots.Find(botId);
                if (botDb == null) throw new Exception("Такого бота не существует.");
                var botUsersCount = _dbContext.BotUsers.Count(botUser => botUser.BotUsername == botDb.BotName);
                if (botUsersCount == 0) throw new Exception("У бота пока нет ни одного пользователя.");
                var errorsCount = await _botMassMailingService.MakeMassMailing(botDb, viewModel);
                return RedirectToAction("Success", "StaticMessage", new { message = $"Массовая рассылка успешно завершена. Количество пользователей, отказавшихся от рассылки: {errorsCount} из {botUsersCount}." });
            }
            catch (Exception e)
            {
                return RedirectToAction("Failure", "StaticMessage", new {message=e.Message});
            }
        }
      
    }
    public class BotMassMailingViewModel
    {
        public string Text { get; set; }
        public IFormFile File{ get; set; }
    }

 
}    