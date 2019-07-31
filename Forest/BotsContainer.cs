using LogicalCore;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace DeleteMeWebhook
{
	public static class BotsContainer
    {
        //BotUsername + BotWrapper
        public static Dictionary<string, BotWrapper> BotsDictionary { get; private set; } = new Dictionary<string, BotWrapper>();
    }
}
