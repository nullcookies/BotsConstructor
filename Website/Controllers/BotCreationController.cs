using System;
using System.Runtime.Serialization;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;
using Website.ViewModels;
using Telegram.Bot;

namespace Website.Controllers
{
    public class BotCreationController : Controller
    {
        readonly StupidLogger _logger;
        readonly ApplicationContext _contextDb;

        public BotCreationController(ApplicationContext contextDb, StupidLogger logger)
        {
            _contextDb = contextDb;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("BotTypeSelection");
        }

        [HttpGet]
        public IActionResult BotForSalesTokenEntry()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult CreateNewBotForSales(TokenChange tokenModel)
        {

            int accountId = (int)HttpContext.Items["accountId"];

            try
            {
                string token = tokenModel?.Token;
                string botUsername = new TelegramBotClient(token).GetMeAsync().Result.Username;

                //Создание нового бота для продаж с пустой разметкой
                BotDB bot = new BotDB() { OwnerId = accountId, BotType = "BotForSales", Token = token, BotName = botUsername };

                _contextDb.Bots.Add(bot);

                //Создание статистики для бота
                BotForSalesStatistics botForSalesStatistics = new BotForSalesStatistics() { Bot = bot, NumberOfOrders = 0, NumberOfUniqueMessages = 0, NumberOfUniqueUsers = 0 };

                _contextDb.BotForSalesStatistics.Add(botForSalesStatistics);

                try
                {
                    _contextDb.SaveChanges();
                }
                catch(Exception exception)
                {
                    throw new TokenMatchException("Возможно в базе уже есть этот бот",exception);
                }
                
				return RedirectToAction("SalesTreeEditor", "BotForSalesEditing", new {botId= bot.Id });

            }
            catch (TokenMatchException ex)
            {
                _logger.Log(LogLevel.USER_ERROR, Source.WEBSITE, $"Сайт. Создание нового бота. При " +
                   $"запросе botUsername было выброшено исключение (возможно, введённый" +
                   $"токен был специально испорчен)" + ex.Message, accountId: accountId);

                ModelState.AddModelError("", "Этот бот уже зарегистрирован.");

            }
            catch (Exception ee)
            {
                _logger.Log(LogLevel.USER_ERROR, Source.WEBSITE, $"Сайт. Создание нового бота. При " +
                    $"запросе botUsername было выброшено исключение (возможно, введённый" +
                    $"токен был специально испорчен)"+ee.Message, accountId:accountId);

                ModelState.AddModelError("", "Ошибка обработки токена.");
            }
           

            return View("BotForSalesTokenEntry");
        }
    }

    
    public class TokenMatchException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

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
}
