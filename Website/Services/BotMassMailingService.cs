using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Website.Controllers.Private_office.My_bots;
using Message = Telegram.Bot.Types.Message;

namespace Website.Services
{
    public class BotMassMailingService
    {
        private readonly ApplicationContext dbContext;

        public BotMassMailingService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task MakeMassMailing(int botId, BotMassMailingViewModel model, List<int> stubUSerIds = null)
        {
            var botDb = dbContext.Bots.Find(botId);
            if(botDb == null) throw new Exception("Bot was null.");

            string token = botDb.Token;

            List<int> botUsers = dbContext.BotUsers
                .Where(botUser => botUser.BotUsername == botDb.BotName)
                .Select(botUser => botUser.BotUserTelegramId)
                .ToList();

            if (stubUSerIds != null)
            {
                botUsers = stubUSerIds;
            }

            if (botUsers.Count == 0) return;

            TelegramBotClient bot = new TelegramBotClient(token);

            if (model.File == null)
            {
                foreach (var userId in botUsers)
                {
                    if (model.File == null)
                    {
                        await bot.SendTextMessageAsync(userId, model.Text);
                    }
                }
            }
            else
            {
                var fileType = model.File.ContentType.Split('/').FirstOrDefault();
                Func<int, InputOnlineFile, string, Task<Message>> senderFunc;
                switch (fileType)
                {
                    case "image":
                        senderFunc = (userId, file, text) => bot.SendPhotoAsync(userId, file, text, ParseMode.Markdown);
                        break;
                    case "audio":
                        senderFunc =
                            ((userId, file, text) => bot.SendAudioAsync(userId, file, text, ParseMode.Markdown));
                        break;
                    case "video":
                        senderFunc = ((userId, file, text) =>
                            bot.SendVideoAsync(userId, file, caption: text, parseMode: ParseMode.Markdown));
                        break;
                    default:
                        senderFunc = ((userId, file, text) =>
                            bot.SendDocumentAsync(userId, file, text, ParseMode.Markdown));
                        break;
                }

                InputOnlineFile onlineFile;

                var firstUserId = botUsers.First();
                using (var stream = model.File.OpenReadStream())
                {
                    onlineFile = new InputOnlineFile(stream);
                    var msg = await senderFunc(firstUserId, onlineFile, model.Text);

                    switch (msg.Type)
                    {
                        case Telegram.Bot.Types.Enums.MessageType.Photo:
                            onlineFile = new InputOnlineFile(msg.Photo.First().FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendPhotoAsync(userId, file, text, ParseMode.Markdown);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Audio:
                            onlineFile = new InputOnlineFile(msg.Audio.FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendAudioAsync(userId, file, text, ParseMode.Markdown);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Video:
                            onlineFile = new InputOnlineFile(msg.Video.FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendVideoAsync(userId, file, caption: text, parseMode: ParseMode.Markdown);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Voice:
                            onlineFile = new InputOnlineFile(msg.Voice.FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendVoiceAsync(userId, file, text, ParseMode.Markdown);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Document:
                            onlineFile = new InputOnlineFile(msg.Document.FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendDocumentAsync(userId, file, text, ParseMode.Markdown);
                            break;
                        case Telegram.Bot.Types.Enums.MessageType.Sticker:
                            onlineFile = new InputOnlineFile(msg.Sticker.FileId);
                            senderFunc = (userId, file, text) =>
                                bot.SendStickerAsync(userId, file);
                            break;
                        default:
                            throw new NotImplementedException($"Поддержка сообщений типа {msg.Type} не реализована.");
                            //break;
                    }
                }

                foreach (var userId in botUsers.Skip(1))
                {
                    try
                    {
                        await senderFunc(userId, onlineFile, model.Text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

    }
}