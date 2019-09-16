using System.Collections.Generic;
using LogicalCore;

namespace Forest
{
	public static class BotsContainer
    {
        //BotUsername + BotWrapper
        public static Dictionary<string, BotWrapper> BotsDictionary { get; private set; } = new Dictionary<string, BotWrapper>();
    }
}
