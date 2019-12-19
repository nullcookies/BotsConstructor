using System;
using System.Linq;
using System.Runtime.Serialization;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Website.ViewModels;
using Telegram.Bot;

namespace Website.Controllers
{
    public class BotCreationController : Controller
    {
        readonly SimpleLogger logger;
        readonly ApplicationContext contextDb;
        private readonly IStringLocalizer localizer;

        public BotCreationController(ApplicationContext contextDb, 
            SimpleLogger logger, 
            IStringLocalizer<BotCreationController> localizer)
        {
            this.contextDb = contextDb;
            this.logger = logger;
            this.localizer = localizer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("BotTypeSelection");
        }

        [HttpGet]
        public IActionResult BotForSalesTokenEntry(BotType botType)
        {
            ViewData["botType"] = botType;
            return View();
        }
        
        [HttpPost]
        public IActionResult CreateNewBotForSales(TokenChange tokenModel, BotType botType)
        {
            int accountId = (int)HttpContext.Items["accountId"];
            
            try
            {
                string token = tokenModel?.Token;
                string botUsername = new TelegramBotClient(token).GetMeAsync().Result.Username;
                string jsonBotMarkup = localizer[botType.ToString()];
                
                int? statusGroupId = contextDb.OrderStatusGroups.FirstOrDefault(stat => stat.OwnerId==accountId)?.Id;
                
                if (statusGroupId == null)
                {
                    //нужно создать группу статусов
                    var orderStatusGroup = new OrderStatusGroup
                    {
                        Name = "Автоматически сгенерированная группа статусов",
                        IsOld = false,
                        OwnerId = accountId
                    };
                    contextDb.OrderStatusGroups.Add(orderStatusGroup);
                    contextDb.SaveChanges();
                    statusGroupId = orderStatusGroup.Id;

                    var orderStatus = new OrderStatus
                    {
                        Message = "Просмотрено",
                        Name = "Просмотрено",
                        GroupId = orderStatusGroup.Id,
                        IsOld = false
                    };
                    contextDb.OrderStatuses.Add(orderStatus);
                    contextDb.SaveChanges();
                }
                
                
                //нужно установить групппу статусов
                if (jsonBotMarkup.Contains("1000001"))
                {
                    jsonBotMarkup = jsonBotMarkup.Replace("1000001", statusGroupId.Value.ToString());
                }
                
                
                BotDB bot = new BotDB
                {
                    OwnerId = accountId,
                    BotType = "BotForSales",
                    Token = token,
                    BotName = botUsername,
                    Markup = jsonBotMarkup
                };

                contextDb.Bots.Add(bot);

                //Создание статистики для бота
                BotForSalesStatistics botForSalesStatistics = new BotForSalesStatistics
                {
                    Bot = bot, NumberOfOrders = 0, NumberOfUniqueMessages = 0, NumberOfUniqueUsers = 0
                };

                contextDb.BotForSalesStatistics.Add(botForSalesStatistics);

                try
                {
                    contextDb.SaveChanges();
                }
                catch(Exception exception)
                {
                    throw new TokenMatchException("Возможно в базе уже есть этот бот",exception);
                }
                
				return RedirectToAction("SalesTreeEditor", "BotForSalesEditing", new {botId= bot.Id });

            }
            catch (TokenMatchException ex)
            {
                logger.Log(LogLevel.USER_ERROR, Source.WEBSITE, $"Сайт. Создание нового бота. При " +
                   $"запросе botUsername было выброшено исключение (возможно, введённый" +
                   $"токен был специально испорчен)" + ex.Message, accountId: accountId);

                ModelState.AddModelError("", "Этот бот уже зарегистрирован.");

            }
            catch (Exception ee)
            {
                logger.Log(LogLevel.USER_ERROR, Source.WEBSITE, $"Сайт. Создание нового бота. При " +
                    $"запросе botUsername было выброшено исключение (возможно, введённый" +
                    $"токен был специально испорчен)"+ee.Message, accountId:accountId);

                ModelState.AddModelError("", "Ошибка обработки токена.");
            }
           

            return View("BotForSalesTokenEntry");
        }
    }

    
    public class TokenMatchException : Exception
    {
      
        public TokenMatchException()
        {
        }

        public TokenMatchException(string message) : base(message)
        {
        }

        public TokenMatchException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TokenMatchException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public enum BotType:byte
    {
        GROUP,
        PIZZERIA,
        ORGANIZATION,
        PROPOSAL,
        PERSONAL,
        EMPTY
    }
    
}
