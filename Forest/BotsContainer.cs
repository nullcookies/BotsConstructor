using System.Collections.Generic;
using LogicalCore;

namespace Forest
{
	public static class BotsContainer
    {
        //BotUsername + BotWrapper
        public static Dictionary<string, BotWrapper> BotsDictionary { get; private set; } = new Dictionary<string, BotWrapper>();
        
        public static void RunAndRegisterBot(BotWrapper botWrapper)
        {
            botWrapper.Run();

            string botUsername = botWrapper.BotClient.GetMeAsync().Result.Username;
            BotsContainer.BotsDictionary.Add(botUsername, botWrapper);
        }
        
    }
}
