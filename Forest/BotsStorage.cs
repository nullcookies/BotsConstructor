using System.Collections.Generic;
using LogicalCore;

namespace Forest
{
	public static class BotsStorage
    {
        //BotUsername + BotWrapper
        public static Dictionary<string, IBot> BotsDictionary { get; } = new Dictionary<string, IBot>();
        
        public static void RunAndRegisterBot(IBot botWrapper)
        {
            botWrapper.Run();

            string botUsername = botWrapper.BotClient.GetMeAsync().Result.Username;
            BotsDictionary.Add(botUsername, botWrapper);
        }
        
    }
}
