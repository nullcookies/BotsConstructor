﻿using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Monitor.TelegramAgent
{
    /// <summary>
    /// Этот бот нужен для того, чтобы присылать телеграм агенту код, который приходит лично пользователю при запросе
    /// на запуск телеграм агента
    /// </summary>
    public class TelegramAgentHelperBot
    {
        const string TELEGRAM_BOT_TOKEN = "968163861:AAGFdJs30-4EZv-2EtoivZClksYGoomQcBM";
        
        readonly TelegramBotClient _telegramBotClient;
        private readonly Monitor.TelegramAgent.MyTelegramAgent _myTelegramAgent;
        
        public TelegramAgentHelperBot(Monitor.TelegramAgent.MyTelegramAgent myTelegramAgent)
        {
            _myTelegramAgent = myTelegramAgent;

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
            Console.WriteLine("Новое сообщение у бота");
            
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

                    try
                    {
                        _myTelegramAgent.SendCodeAsync().Wait();
                        _telegramBotClient.SendTextMessageAsync(senderId, "Телеграм агент запускается." +
                                                                          " Отправьте мне код для его запуска");
                    }catch(Exception exception)
                    {
                        _telegramBotClient.SendTextMessageAsync(senderId, $"Что-то пошло не так. Ошибка:{exception.Message}");
                    }
              
                    break;
                default:
                    try
                    {
                        _myTelegramAgent.MakeAuth(messageText);
                    }
                    catch (Exception exception)
                    {
                        _telegramBotClient.SendTextMessageAsync(senderId, $"Что-то пошло не так. Ошибка:{exception.Message}");
                    }
                    break;
            }
            

        }

    }
}   

