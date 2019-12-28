using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Other.Filters;
using Website.Services;

namespace Website.Controllers.Private_office.My_bots
{
    public class BotMassMailingController:Controller
    {
        private readonly BotMassMailingService botMassMailingService;

        public BotMassMailingController(BotMassMailingService botMassMailingService)
        {
            this.botMassMailingService = botMassMailingService;
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
                await botMassMailingService.MakeMassMailing(botId, viewModel);
            }
            catch (Exception e)
            {
                return RedirectToAction("Failure", "StaticMessage", new {message=e.Message});
            }
            
            
            return RedirectToAction("MyBots", "MyBots");
        }
      
    }
    public class BotMassMailingViewModel
    {
        public string Text { get; set; }
        public IFormFile File{ get; set; }
    }

 
}    