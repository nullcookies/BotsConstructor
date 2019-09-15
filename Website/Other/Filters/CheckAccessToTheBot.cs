using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Models;
using DataLayer.Services;

//15 09 2019 15 44 это выглядит очень плохо

namespace Website.Other.Filters
{
    /// <summary>
    /// Проверяет, что бот с id в запросе принадлежит пользователю
    /// В противном случае вернёт 403/404
    /// </summary>
    public class CheckAccessToTheBot : Attribute, IActionFilter
    {
        ApplicationContext _context;
        StupidLogger _logger;

        public CheckAccessToTheBot(ApplicationContext context, StupidLogger logger)
        {
            _context = context;
            _logger = logger;
        }

      
        public void OnActionExecuting(ActionExecutingContext context)
        {
            int? botId = context.ActionArguments["botId"] as int?;
            
            if (botId == null)
            {
                //В запросе не был указан botId
                _logger.Log(LogLevelMyDich.UNAUTHORIZED_ACCESS_ATTEMPT, Source.WEBSITE, "В запросе не был указан botId");
                context.Result = new StatusCodeResult(404);
                return;
            }

            var bot = _context.Bots.Find(botId);
			
            if (bot == null)
            {
                //Бота с таким id не существует
                _logger.Log(LogLevelMyDich.UNAUTHORIZED_ACCESS_ATTEMPT, Source.WEBSITE, $"Бота с таким id не существует botId={botId}");
                context.Result = new StatusCodeResult(404);
                return;
            }

            int accountId = int.MinValue;
            try
            {
                accountId = Stub.GetAccountIdFromCookies(context.HttpContext) ?? throw new Exception("Из cookies не удалось извлечь accountId");
            }
            catch
            {
                context.Result = new StatusCodeResult(403);
                _logger.Log(LogLevelMyDich.UNAUTHORIZED_ACCESS_ATTEMPT, Source.WEBSITE, $"Сайт. Из cookies не удалось извлечь accountId. При доступе к боту bot.Id={bot.Id}");
                return;
            }



            int ownerId = bot.OwnerId;
            if (ownerId != accountId)
            {
                //Бот не принадлежит этому пользователю
                _logger.Log(LogLevelMyDich.UNAUTHORIZED_ACCESS_ATTEMPT, Source.WEBSITE, $"Бот не принадлежит этому пользователю. accountId={accountId}, ownerId={ownerId}");

                context.Result = new StatusCodeResult(403);
                return;
            }

           //Ок
           //Этот пользователь имеет право доступа к боту

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
           
        }
    }
}
