using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using MyLibrary;
using Website.Other.Filters;

//TODO  проверка прав доступа
namespace Website.Controllers
{
	
    [Authorize]
	public class BotForSalesEditingController : Controller
    {
        readonly ApplicationContext _context;
        private readonly SimpleLogger _logger;
        
        public BotForSalesEditingController(ApplicationContext context, SimpleLogger logger)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

		[HttpGet]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SalesTreeEditor(int botId)
		{
			var info = _context.Bots.Where((_bot) => _bot.Id == botId).Select((_bot) => new { _bot.Owner.TelegramId, _bot.Token, _bot.Markup, _bot.OwnerId }).SingleOrDefault();
            var statusGroups = _context.OrderStatusGroups.Where(group => group.OwnerId == info.OwnerId)
                .Select(group => new {group.Id, group.Name, group.IsOld}).ToDictionary(group => group.Id, group => new {group.Name, group.IsOld});

            if (info != null)
            {
	            ViewData["userId"] = info.TelegramId;
				ViewData["token"] = info.Token;
				ViewData["json"] = info.Markup;
	            ViewData["statusGroups"] = statusGroups;
	            return View();
            }
            else
            {
				_logger.Log(LogLevel.ERROR, Source.WEBSITE, "Не удалось достать бота из базы для показа" +
	                                                           " страницы редактирования разметки");   
				return StatusCode(500);
            }

		}

		[HttpPost]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SaveTree(int botId, string tree)
		{
			//TODO: Возможно, стоит проверять адекватность содержимого
			_context.Bots.Find(botId).Markup = tree;
			_context.SaveChanges();
			return Ok();
		}
		
		
	
    }
}