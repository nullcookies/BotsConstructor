using DataLayer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Website.Other.Filters;
using Website.TelegramAgent;

//TODO  проверка прав доступа
namespace Website.Controllers
{
    [Authorize]
	public class BotForSalesEditingController : Controller
    {
        readonly ApplicationContext _context;
        IHostingEnvironment _appEnvironment;
        
        private readonly MyTelegramAgent _telegramAgent;

        public BotForSalesEditingController(MyTelegramAgent telegramAgent)
        {
	        _telegramAgent = telegramAgent;
        }

        public BotForSalesEditingController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
        }

		[HttpGet]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public IActionResult SalesTreeEditor(int botId)
		{
			var info = _context.Bots.Where((_bot) => _bot.Id == botId).Select((_bot) => new { _bot.Owner.TelegramId, _bot.Token, _bot.Markup, _bot.OwnerId }).SingleOrDefault();
            var statusGroups = _context.OrderStatusGroups.Where(group => group.OwnerId == info.OwnerId)
                .Select(group => new {group.Id, group.Name, group.IsOld}).ToDictionary(group => group.Id, group => new {group.Name, group.IsOld});
			ViewData["userId"] = info.TelegramId;
			ViewData["token"] = info.Token;
			ViewData["json"] = info.Markup;
            ViewData["statusGroups"] = statusGroups;
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
		
		
		
		[HttpGet]
		[TypeFilter(typeof(CheckAccessToTheBot))]
		public async Task<IActionResult> GetFileId(IFormFile uploadedFile)
		{
			
			try
			{
				Stream stream = uploadedFile.OpenReadStream();
				await _telegramAgent.MySendPhoto(stream);
			}
			catch (Exception eee)
			{
				return Content("не работает. "+eee.Message);
			}

			return Content("работает");
			
		}
    }
}