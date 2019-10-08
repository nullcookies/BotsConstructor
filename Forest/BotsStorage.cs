using System.Collections.Generic;
using LogicalCore;

namespace Forest
{
	public static class BotsStorage
    {
        //BotUsername + BotWrapper
        public static Dictionary<string, BotWrapper> BotsDictionary { get; private set; } = new Dictionary<string, BotWrapper>();
        
        public static void RunAndRegisterBot(BotWrapper botWrapper)
        {
            botWrapper.Run();

            string botUsername = botWrapper.BotClient.GetMeAsync().Result.Username;
            BotsStorage.BotsDictionary.Add(botUsername, botWrapper);
        }
        
    }
}
