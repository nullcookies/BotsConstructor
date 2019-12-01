
using System;
using DataLayer;
using LogicalCore;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;
using Telegram.Bot.Types;

namespace Forest.Controllers
{
    public class TelegramNegotiatorController:Controller
    {
        private readonly ApplicationContext _contextDb;
        private readonly SimpleLogger _logger;
        
        [HttpPost]
        [Route("{telegramBotUsername}")]
        public IActionResult Index([FromBody] Update update)
        {
            string botUsername = RouteData.Values["telegramBotUsername"].ToString();
            if (BotsStorage.BotsDictionary.TryGetValue(botUsername, out BotWrapper botWrapper))
            {
                try
                {
                    botWrapper.AcceptUpdate(update);
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.ERROR, Source.FOREST, $"При обработке сообщения ботом botUsername={botUsername}" + $" через webhook было брошено исключение", ex:exception);
                }
            }
            else
            {
                _logger.Log(LogLevel.WARNING, Source.FOREST, $"Пришло обновление для бота, которого нет в списке онлайн ботов. botUsername={botUsername}");
            }

            return Ok();
        }
    }
}