using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
	public class BotWrapper : EmptyBot
    {
        public MegaTree MegaTree { get; set; }
        private readonly ConcurrentDictionary<int, Session> sessionsDictionary;
        public BotOwner BotOwner { get; private set; }
        public readonly BaseTextMessagesManager tmm;
        public readonly GlobalFilter globalFilter; // Глобальный фильтр сообщений и нажатий, которые выполняются с любого узла
        public readonly VariablesContainer globalVars; // Глобальные переменные, которые видны для всех сессий
		public List<string> Languages => tmm.languages;
		public Action<VariablesContainer> InitializeSessionVars { get; set; } // вызывается для каждой сессии в конструкторе

        public BotStatistics StatisticsContainer;
        public StupidBotAntispam StupidBotAntispam;

        public BotWrapper(int botId, 
            string link, 
            string token,
            /*int ownerID, MegaTree tree,*/
            TextMessagesManager textManager = null,
			GlobalFilter filter = null, 
            VariablesContainer globalVariables = null
            
            ) : base(botId, link, token)
		{
			sessionsDictionary = new ConcurrentDictionary<int, Session>();
			tmm = textManager ?? new BaseTextMessagesManager();
			//MegaTree = tree ?? throw new ArgumentNullException(nameof(tree));
			globalFilter = filter ?? new GlobalFilter();
			globalVars = globalVariables ?? new VariablesContainer();
            //BotOwner = new BotOwner(ownerID, this);

		}

		public void SetOwner(int ownerID)
		{
			BotOwner = new BotOwner(ownerID, this);
		}

        public override void Stop()
        {
            base.Stop();
        }

        public override void AcceptUpdate(Update update)
        {
            switch (update.Type)
            {
                case (UpdateType.Message):
                    AcceptMessage(update.Message);
                    break;
                case (UpdateType.CallbackQuery):
                    AcceptCallbackQuery(update.CallbackQuery);
                    break;
                default:
                    ConsoleWriter.WriteLine($"Unexpected update type={update.Type}");
                    break;
            }
        }

     

        protected override void AcceptMessage(Message message)
        {
            int telegramId = message.From.Id;

            Console.WriteLine("Сообщение "+message.Text);
            try
            {
                Session session = GetSessionByTelegramId(telegramId);
                session.TakeControl(message);
            }
            catch
            {
                Console.WriteLine("Some dich");
            }
        }

        protected override void AcceptCallbackQuery(CallbackQuery callbackQuerry)
        {
            int telegramId = callbackQuerry.From.Id;


            Session session = GetSessionByTelegramId(telegramId);
            session.TakeControl(callbackQuerry);
        }

        public bool TryGetSessionByTelegramId(int id, out Session session)
        {
            bool chatIsFound = BotClient.GetChatAsync(id).ContinueWith((chatTask) => chatTask.IsCompletedSuccessfully).Result;

            if (chatIsFound)
            {
                session = GetSessionByTelegramId(id);
            }
            else
            {
                session = null;
            }

            return chatIsFound;
        }

        public Session GetSessionByTelegramId(int id)
        {
            Session session = sessionsDictionary.GetOrAdd(id, new Session(MegaTree.root, id, this));            

            if (BotOwner != null && BotOwner.Session == null && BotOwner.id == id)
            {
                BotOwner.Session = session;
            }
            return session;
        }
    }

    public class BotStatistics
    {
        public List<int> usersTelegramIds = new List<int>();
        public int NumberOfMessages;
    }    
    public class StupidBotAntispam
    {
        List<int> blocketUsersIds = new List<int>();

        public bool UserIsBlockedForThisBot(int userTelegramId)
        {
            return blocketUsersIds.Contains(userTelegramId);

        }

        
    }
}



