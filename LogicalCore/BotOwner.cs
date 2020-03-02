using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace LogicalCore
{
	public class BotOwner
    {
        public readonly int id;
		public readonly Dictionary<string, Action<Session>> actions;
        private readonly BotWrapper botWrapper;
        private ITelegramBotClient BotClient => botWrapper.BotClient;
        public Session GetSessionById(int sessionId) => botWrapper.GetSessionByTelegramId(sessionId);
        private Session session;
        public Session Session
        {
            get => session;

            set
            {
                session = value;

                BotClient.SendTextMessageAsync(id, session.Translate("HelloForOwner"));
            }
        }

        public BotOwner(int ownerID, BotWrapper botWrapper)
        {
            id = ownerID;
			actions = new Dictionary<string, Action<Session>>();
			this.botWrapper = botWrapper;
            if (botWrapper.TryGetSessionByTelegramId(ownerID, out Session session))
            {
                Session = session;
            }
			else
			{
				throw new Exception("Сессия владельца бота не обнаружена!");
			}
        }
    }
}
