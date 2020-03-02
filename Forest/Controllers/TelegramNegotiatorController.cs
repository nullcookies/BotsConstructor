
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
        private readonly ApplicationContext contextDb;
        private readonly SimpleLogger logger;

        public TelegramNegotiatorController(ApplicationContext contextDb, SimpleLogger logger)
        {
            this.contextDb = contextDb;
            this.logger = logger;
        }

        [HttpPost]
        [Route("{telegramBotUsername}")]
        public IActionResult Index([FromBody] Update update)
        {
            string botUsername = RouteData.Values["telegramBotUsername"].ToString();
            if (BotsStorage.BotsDictionary.TryGetValue(botUsername, out var botWrapper))
            {
                try
                {
                    botWrapper.AcceptUpdate(update);
                    if (update?.Message?.Text != null)
                    {
                        logger.Log(LogLevel.TELEGRAM_MESSAGE, Source.FOREST, $"телеграм сообщение от ползователя с Id={update?.Message?.From?.Id} text={update.Message.Text}" );
                    }
                    
                }
                catch (Exception exception)
                {
                    logger.Log(LogLevel.ERROR, Source.FOREST, $"При обработке сообщения ботом botUsername={botUsername}" + $" через webhook было брошено исключение", ex:exception);
                }
            }
            else
            {
                logger.Log(LogLevel.WARNING, Source.FOREST, $"Пришло обновление для бота, которого нет в списке онлайн ботов. botUsername={botUsername}");
            }

            return Ok();
        }
    }
}