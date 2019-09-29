using System;
using DataLayer;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineQueryResults;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Website.Areas.Monitor.Services
{
    public class BotForAgent
    {
        readonly StupidLogger _logger;
        readonly DbContextFactory _dbContextFactory;
        readonly TelegramBotClient _telegramBotClient;

        private readonly TestTelegramApi _testTelegramApi;


        const string TELEGRAM_BOT_TOKEN = "968163861:AAGFdJs30-4EZv-2EtoivZClksYGoomQcBM";


        public BotForAgent(StupidLogger simpleLogger, IConfiguration configuration, TestTelegramApi testTelegramApi)
        {
            _logger = simpleLogger;
            _testTelegramApi = testTelegramApi;
            



            _telegramBotClient = new TelegramBotClient(TELEGRAM_BOT_TOKEN);

            _telegramBotClient.OnMessage += _HandleMessage;

            _telegramBotClient.StartReceiving(
                new Telegram.Bot.Types.Enums.UpdateType[]
                {
                    Telegram.Bot.Types.Enums.UpdateType.Message
                });
        }

        private void _HandleMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            int senderId = message.From.Id;

            //Сообщения принимаются только от строго
            //определённых аккаунтов
            if (senderId != 440090552)
                return;

            //Боту пишет админ
            //Он может 
            //1) запросить код для авторизации
            //2) ввести этот код 



            string messageText = message.Text;

            switch (messageText)
            {
                case ("/requestCode"):
                    //TODO запросить код через api
                    break;
                default:
                    //TODO попытаться отправить код
                    break;
            }

            _telegramBotClient.SendTextMessageAsync(message.Chat.Id,
                $"");


        }

    }
}   

