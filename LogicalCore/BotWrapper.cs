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

        public BotStatistics StatisticsContainer = new BotStatistics();
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
            //TODO Положить сюда обновление счётчика сообщений
            //пока нелья, тк оно может запускаться через longpolling
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

            //На случай запуска через long polling
            StatisticsContainer.UpdateStatistics(telegramId);


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


            //На случай запуска через long polling
            StatisticsContainer.UpdateStatistics(telegramId);

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
        private List<int> _usersTelegramIds = new List<int>();
        private int _numberOfMessages;

        //public List<int> GetAllUsersTelegramIds()
        // {
        //     //Зачем тут копирование?
        //     return new List<int>(usersTelegramIds);
        // }

        public int GetNumberOfAllUsers()
        {
            return _usersTelegramIds.Count;
        }

        public List<int> GetNewUsersTelegramIds(HashSet<int> dbUsersTelegramIds)
        {
            List<int> newUsersTelegramIds = new List<int>();

            for (int i = 0; i < _usersTelegramIds.Count; i++)
            {
                int userTelegramId = _usersTelegramIds[i];

                //O(1)
                if (!dbUsersTelegramIds.Contains(userTelegramId))
                {
                    newUsersTelegramIds.Add(userTelegramId);
                }
            }

            return newUsersTelegramIds;
        }
        public long NumberOfMessages
        {
            get
            {
                return _numberOfMessages;
            }
        }

        public void UpdateStatistics(int userTelegramId)
        {
            //кол-во сообщений
            _numberOfMessages++;

            //пользователи
            ConsiderUser(userTelegramId);
        }


        public void ConsiderUser(int userTelegramId)
        {
            bool this_user_is_already_there = _usersTelegramIds
                .Contains(userTelegramId);

            //Новый пользователь
            if (!this_user_is_already_there)
            {
                _usersTelegramIds.Add(userTelegramId);
            }
        }


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



