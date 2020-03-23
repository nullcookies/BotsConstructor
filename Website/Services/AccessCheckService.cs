using System;
using DataLayer;

namespace Website.Services
{
    public class AccessCheckService
    {
        private ApplicationContext dbContext;

        public AccessCheckService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public bool IsAccountCanRunBot(int botId, int accountId)
        {
            throw new NotImplementedException();
        }
        public bool IsAccountCanRunBot(int botId, Account account)
        {
            throw new NotImplementedException();
        }
        public bool IsAccountCanRunBot(BotDB bot, Account account)
        {
            throw new NotImplementedException();
        } 
        public bool IsAccountCanRunBot(BotDB bot, int accountId)
        {
            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }
            if (accountId == default)
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }
            return bot.OwnerId == accountId;
        }
        
        public bool IsAccountCanStopBot(BotDB bot, int accountId)
        {
            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }
            if (accountId == default)
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }
            return bot.OwnerId == accountId;
        }
    }
}