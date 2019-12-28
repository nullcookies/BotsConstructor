using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using Website.Other.Filters;

//TODO  проверка прав доступа
namespace Website.Controllers
{
	
    [Authorize]
	public class BotForSalesEditingController : Controller
    {
        readonly ApplicationContext context;
        private readonly SimpleLogger logger;
        
        public BotForSalesEditingController(ApplicationContext context, SimpleLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger;
        }

		[HttpGet]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SalesTreeEditor(int botId)
		{
			var info = context.Bots
				.Where(_bot => _bot.Id == botId)
				.Select(_bot => new { _bot.Token, _bot.Markup, _bot.OwnerId })
				.SingleOrDefault();
			
			
			
            var statusGroups = context.OrderStatusGroups.Where(group => group.OwnerId == info.OwnerId)
                .Select(group => new {group.Id, group.Name, group.IsOld}).ToDictionary(group => group.Id, group => new {group.Name, group.IsOld});
    
            if (info != null)
            {
	            //TODO узнать telegramId из первого запроса 
	            int telegramId = context.TelegramLoginInfo
		            .Where(loginInfo => loginInfo.AccountId == info.OwnerId)
		            .Select(telInfo => telInfo.TelegramId)
		            .SingleOrDefault();
		            
	            ViewData["userId"] = telegramId;
				ViewData["token"] = info.Token;
				ViewData["json"] = info.Markup;
	            ViewData["statusGroups"] = statusGroups;
	            return View();
            }
            else
            {
				logger.Log(LogLevel.ERROR, Source.WEBSITE, "Не удалось достать бота из базы для показа" +
	                                                           " страницы редактирования разметки");   
				return StatusCode(500);
            }

			
		}

		[HttpPost]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SaveTree(int botId, string tree)
		{
			//TODO: Возможно, стоит проверять адекватность содержимого
			context.Bots.Find(botId).Markup = tree;
			context.SaveChanges();
			return Ok();
		}
		
		
	
    }
}