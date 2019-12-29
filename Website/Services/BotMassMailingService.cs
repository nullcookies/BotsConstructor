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
        private readonly ApplicationContext _dbContext;

        public BotMassMailingService(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> MakeMassMailing(BotDB botDb, BotMassMailingViewModel model)
        {
            var botUsers = _dbContext.BotUsers
                .Where(botUser => botUser.BotUsername == botDb.BotName)
                .Select(botUser => botUser.BotUserTelegramId);


            if (!botUsers.Any()) return 0;

            int errorsCount = 0;

            var bot = new TelegramBotClient(botDb.Token);

            if (model.File == null)
            {
                foreach (var userId in botUsers)
                {
                    try
                    {
                        await bot.SendTextMessageAsync(userId, model.Text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        errorsCount++;
                    }
                }
            }
            else
            {
                var fileType = model.File.ContentType.Split('/').FirstOrDefault();

                InputOnlineFile onlineFile;
                Message msg;

                while (true)
                {
                    try
                    {
                        var firstUserId = botUsers.Skip(errorsCount).First();
                        using (var stream = model.File.OpenReadStream())
                        {
                            onlineFile = new InputOnlineFile(stream);
                            switch (fileType)
                            {
                                case "image":
                                    msg = await bot.SendPhotoAsync(firstUserId, onlineFile, model.Text,
                                        ParseMode.Markdown);
                                    break;
                                case "audio":
                                    msg = await bot.SendAudioAsync(firstUserId, onlineFile, model.Text,
                                        ParseMode.Markdown);
                                    break;
                                case "video":
                                    msg = await bot.SendVideoAsync(firstUserId, onlineFile, caption: model.Text,
                                        parseMode: ParseMode.Markdown);
                                    break;
                                default:
                                    msg = await bot.SendDocumentAsync(firstUserId, onlineFile, model.Text,
                                        ParseMode.Markdown);
                                    break;
                            }
                        }

                        break;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e);
                        return errorsCount;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        errorsCount++;
                    }
                }

                Func<int, Task<Message>> senderFunc;
                switch (msg.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        onlineFile = new InputOnlineFile(msg.Photo.First().FileId);
                        senderFunc = (userId) =>
                            bot.SendPhotoAsync(userId, onlineFile, model.Text, ParseMode.Markdown);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Audio:
                        onlineFile = new InputOnlineFile(msg.Audio.FileId);
                        senderFunc = (userId) =>
                            bot.SendAudioAsync(userId, onlineFile, model.Text, ParseMode.Markdown);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Video:
                        onlineFile = new InputOnlineFile(msg.Video.FileId);
                        senderFunc = (userId) =>
                            bot.SendVideoAsync(userId, onlineFile, caption: model.Text, parseMode: ParseMode.Markdown);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Voice:
                        onlineFile = new InputOnlineFile(msg.Voice.FileId);
                        senderFunc = (userId) =>
                            bot.SendVoiceAsync(userId, onlineFile, model.Text, ParseMode.Markdown);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Document:
                        onlineFile = new InputOnlineFile(msg.Document.FileId);
                        senderFunc = (userId) =>
                            bot.SendDocumentAsync(userId, onlineFile, model.Text, ParseMode.Markdown);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Sticker:
                        onlineFile = new InputOnlineFile(msg.Sticker.FileId);
                        senderFunc = (userId) =>
                            bot.SendStickerAsync(userId, onlineFile);
                        break;
                    default:
                        throw new NotImplementedException($"Поддержка сообщений типа {msg.Type} не реализована.");
                }

                foreach (var userId in botUsers.Skip(errorsCount + 1))
                {
                    try
                    {
                        await senderFunc(userId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        errorsCount++;
                    }
                }
            }

            return errorsCount;
        }
    }
}