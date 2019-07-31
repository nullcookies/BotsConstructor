using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Other.Filters
{
    /// <summary>
    /// Проверяет, что бот с id в запросе принадлежит пользователю
    /// В противном случае вернёт 403/404
    /// </summary>
    public class CheckAccessToTheBot : Attribute, IActionFilter
    {
        ApplicationContext _context;

        public CheckAccessToTheBot(ApplicationContext context)
        {
            _context = context;
        }

        //Как тут применить это чудо?
        //[FromQuery(Name ="botId"];

        public void OnActionExecuting(ActionExecutingContext context)
        {
            int botId = int.MinValue;
                                    
            string requestParameter = context.HttpContext.Request.Query["botId"];
            
            if (!int.TryParse(requestParameter, out botId))
            {
                //В запросе не был указан botId
                //context.Result = new StatusCodeResult(404);
                //context.Result = new Result(" у вас нет прав на редактирование этого бота") ;
                context.Result = new StatusCodeResult(404);
                return;
            }

            var bot = _context.Bots.Find(botId);

             
            if (bot == null)
            {
                //Бота с таким id не существует
                context.Result = new StatusCodeResult(404);
                return;
            }

            int ownerId = bot.OwnerId;

            string login = context
              .HttpContext
              .User
              .Claims
              .Where(claim => claim.Type == claim.Subject.NameClaimType)
              .Select(claim => claim.Value)
              .FirstOrDefault();

            
            
            int accountId = 0;

            try
            {
                accountId = Stub.GetAccountIdByHttpContext(context.HttpContext, _context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }
            catch
            {
                return ;
            }



            if (ownerId != accountId)
            {
                //Бот не принадлежит этому пользователю
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
