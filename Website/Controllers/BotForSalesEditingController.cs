using DataLayer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Website.Other.Filters;

//TODO  проверка права доступа
namespace Website.Controllers
{
    [Authorize]
	public class BotForSalesEditingController : Controller
    {
        readonly ApplicationContext _context;
        IHostingEnvironment _appEnvironment;

        public BotForSalesEditingController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
        }

		[HttpGet]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SalesTreeEditor(int botId)
		{
			var info = _context.Bots.Where((_bot) => _bot.Id == botId).Select((_bot) => new { _bot.Owner.TelegramId, _bot.Token, _bot.Markup }).SingleOrDefault();
			ViewData["userId"] = info.TelegramId;
			ViewData["token"] = info.Token;
			ViewData["json"] = info.Markup;
			return View();
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