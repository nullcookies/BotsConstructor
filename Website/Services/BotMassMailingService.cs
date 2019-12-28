using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Website.Controllers.Private_office.My_bots;

namespace Website.Services
{
    public class BotMassMailingService
    {
        private readonly ApplicationContext dbContext;

        public BotMassMailingService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task MakeMassMailing(int botId, BotMassMailingViewModel model, List<int> stubUSerIds)
        {
            string token = dbContext.Bots.SingleOrDefault(botDb => botDb.Id == botId)?.Token;
            if (token==null)
                throw new Exception();

            List<int> botUsers = dbContext.BotUsers
                .Where(botUser => botUser.BotUserTelegramId == botId)
                .Select(botUser => botUser.BotUserTelegramId)
                .ToList();

            if (stubUSerIds != null)
            {
                botUsers = stubUSerIds;
            }
            
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
            }else if(model.File.ContentType.Contains("image"))
            {
                
                foreach (var userId in botUsers)
                {
                    using (var stream = model.File.OpenReadStream())
                    {
                        InputOnlineFile photo = new InputOnlineFile(stream);
                        await bot.SendPhotoAsync(userId, photo, model.Text);
                    }
                }
                
                
            }


        }
    }
}