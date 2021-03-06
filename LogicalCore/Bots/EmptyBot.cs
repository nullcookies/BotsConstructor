﻿using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
    /// <summary>
    /// Сущность бота. Может принимать сообщения через webhook и long polling.
    /// При надобности можно перезрузить методы обработки сообщений.
    /// </summary>
    public class EmptyBot : IBot
	{
        /// <summary>
        /// Для запуска в режиме webhook
        /// </summary>
        /// <param name="link"></param>
        /// <param name="token"></param>
        public EmptyBot(int botId, string link, string token) 
        {
			BotId = botId;
			this.link = link;
            BotClient = new TelegramBotClient(token);
            BotUsername = BotClient.GetMeAsync().Result.Username;
        }

        /// <summary>
        /// ID бота в БД.
        /// </summary>
        public int BotId { get; }


        /// <summary>
        /// Ссылка для вебхука, если он нужен
        /// </summary>
        protected readonly string link;

        public string BotUsername { get; }
        public ITelegramBotClient BotClient { get; }

        #region Запуск принятия сообщений

        /// <summary>
        /// Запуск принятия сообщений
        /// </summary>
        public virtual void Run()
        {
            // RunLongPolling(); 
            if (link == null)
            {
                RunLongPolling();
            }
            else
            {
                try
                {
                    RunWebhook(link);
                    ConsoleWriter.WriteLine($"Бот {BotUsername} запущен в режиме Webhook ", ConsoleColor.Green);
                }
                catch (Exception)
                {
                    RunLongPolling();
                }
            }
        }

        /// <summary>
        /// Сообщения присылаются серверами телеграма без постоянного опроса
        /// </summary>
        protected virtual void RunWebhook(string link)
        {
            BotClient.DeleteWebhookAsync().Wait();

            List<UpdateType> allowedUpdates = new List<UpdateType>()
                {
                    UpdateType.CallbackQuery,
                    UpdateType.Message,
                    UpdateType.EditedMessage
                };
            try
            {
                BotClient.SetWebhookAsync(link, allowedUpdates: allowedUpdates).Wait();
                PrintStartMessage();
            }
            catch (Exception ee)
            {
                ConsoleWriter.WriteLine("Не удалось установить webhook по ссылке " + link, ConsoleColor.Red);
                ConsoleWriter.WriteLine(ee.Message, ConsoleColor.Red);
                throw;
            }
        }
        
        /// <summary>
        /// Принятие сообщений при постоянном опросе серверов телеграма
        /// </summary>
        protected virtual void RunLongPolling()
        {
            BotClient.DeleteWebhookAsync().Wait();

            AttachHandlers();

            BotClient.StartReceiving();

            PrintStartMessage();
        }

        /// <summary>
        /// Перед запуском в режиме long polling (постоянный опрос) нужно указать сообщения какого типа нужно принимать
        /// </summary>
        protected virtual void AttachHandlers()
        {
            BotClient.OnMessage += BotOnMessageReceived;
            BotClient.OnMessageEdited += BotOnMessageReceived;
            BotClient.OnCallbackQuery += BotOnCallbackQueryReceived;
        }

        protected virtual void PrintStartMessage()
        {
            ConsoleWriter.WriteLine($"Start listening for @{BotUsername}", ConsoleColor.Green);
        }

        //TODO Это точно лучший метод остановить принятие сообщений?
        public virtual void Stop()
        {
            if (link == null)
            {
                //бот работал в режиме long polling
                BotClient.StopReceiving();
            }
            else
            {
                //бот работал в режиме webhook
                BotClient.DeleteWebhookAsync();
            }
        }
        #endregion

        #region Принятие сообщения

        public virtual void AcceptUpdate(Update update)
        {
            switch (update.Type)
            {
                case (UpdateType.Message):
                    AcceptMessage(update.Message);
                    break;
                case (UpdateType.CallbackQuery):
                    AcceptCallbackQuery(update.CallbackQuery);
                    break;
                default:
                    ConsoleWriter.WriteLine($"Unexpected update type={update.Type}");
                    break;
            }
        }

		protected virtual void AcceptMessage(Message message)
		{
			string logMessage = $"{ message.From.FirstName} { message.From.LastName} " +
			   $"(nick = {message.From.Username}) " +
			   $"(Id =  {message.From.Id}) " +
			   $"(message type = {message.Type.ToString()}) " +
			   $"(date = {message.Date}): { message.Text }.";

			ConsoleWriter.WriteLine(logMessage);
		}

		protected virtual void AcceptCallbackQuery(CallbackQuery callbackQuerry)
		{
			string buttonData = callbackQuerry.Data;
			string name = $"{callbackQuerry.From.FirstName} {callbackQuerry.From.LastName}";
			string logMessage = $"{name} нажал кнопку {buttonData}";

			ConsoleWriter.WriteLine(logMessage);
		}

        protected virtual void BotOnMessageReceived(object sender, MessageEventArgs eventArgs)
        {
            AcceptMessage(eventArgs.Message);
        }

        protected virtual void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs eventArgs)
        {
            AcceptCallbackQuery(eventArgs.CallbackQuery);
        }
        #endregion
    }
}



