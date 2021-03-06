﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DataLayer;
using MyLibrary;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
    public class BotWrapper : EmptyBot, IExtendedBot
    {
        public IMarkupTree MarkupTree { get; set; }
        private readonly ConcurrentDictionary<int, ISession> sessionsDictionary;
        public BotOwner BotOwner { get; private set; }
        public ITextMessagesManager TMM { get; }
        public IFilter GlobalFilter { get; } // Глобальный фильтр сообщений и нажатий, которые выполняются с любого узла
        public IVariablesContainer GlobalVars { get; } // Глобальные переменные, которые видны для всех сессий
        public List<string> Languages => TMM.Languages;
        public Action<IVariablesContainer> InitializeSessionVars { get; set; } // вызывается для каждой сессии в конструкторе
        public BotStatisticsForest StatisticsContainer { get; }
        private readonly StupidBotAntispam stupidBotAntispam;

        public BotWrapper(int botId,
            string link,
            string token,
            /*int ownerID, IMarkupTree tree,*/
            ITextMessagesManager textManager = null,
            IFilter filter = null,
            IVariablesContainer globalVariables = null,
            BotStatisticsForest botStatistics = null,
            StupidBotAntispam antispam = null

            ) : base(botId, link, token)
        {

            StatisticsContainer = botStatistics ?? new BotStatisticsForest();
            stupidBotAntispam = antispam ?? new StupidBotAntispam();
            sessionsDictionary = new ConcurrentDictionary<int, ISession>();
            TMM = textManager ?? new UntranslatableTextMessagesManager();
            //MarkupTree = tree ?? throw new ArgumentNullException(nameof(tree));
            GlobalFilter = filter ?? new GlobalFilter();
            GlobalVars = globalVariables ?? new VariablesContainer();
            //BotOwner = new BotOwner(ownerID, this);

        }

        public void SetOwner(int ownerId)
        {
            BotOwner = new BotOwner(ownerId, this);
        }

        public override void Stop()
        {
            LoggerSingelton.GetLogger().Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                $"Остановка бота. BotUsername={BotUsername}");
            
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
            

            
            //Шпионское логгирование
            LoggerSingelton.GetLogger().Log(
                LogLevel.INFO,
                Source.OTHER,
                $"Message, BotUsername={BotUsername}, senderId={message.From.Id}, type ={message.Type}, text={message.Text}"
            );
            
            bool isBlocked = stupidBotAntispam.UserIsBlockedForThisBot(telegramId);
            if (isBlocked)
            {
                //TODO ответить, что заблокирован
                BotClient.SendTextMessageAsync(message.Chat.Id,"это же бан");
                return;
                
            }
            
            try
            {
                var session = GetSessionByTelegramId(telegramId);
                session.TakeControl(message);
            }
            catch(Exception exception)
            {
                LoggerSingelton.GetLogger().Log(
                    LogLevel.ERROR,
                    Source.FOREST,
                    $"При обработке сообщения для бота BotUsername= {BotUsername} через long polling было брошено исключение",
                    ex:exception
                );
            }
        }


        protected override void AcceptCallbackQuery(CallbackQuery callbackQuerry)
        {
            int telegramId = callbackQuerry.From.Id;

            //На случай запуска через long polling
            StatisticsContainer.UpdateStatistics(telegramId);

            
                   
            //Шпионское логгирование
            LoggerSingelton.GetLogger().Log(
                LogLevel.INFO,
                Source.OTHER,
                $"CallbackQuery, BotUsername={BotUsername}, senderId={callbackQuerry.Message.From.Id}, text={callbackQuerry.Data}"
            );
            
            
            bool isBlocked = stupidBotAntispam.UserIsBlockedForThisBot(telegramId);
            if (isBlocked)
            {
                //TODO ответить, что заблокирован
                BotClient.SendTextMessageAsync(callbackQuerry.Message.Chat.Id, "это же бан");
                return;
            }

            var session = GetSessionByTelegramId(telegramId);
            session.TakeControl(callbackQuerry);
        }



        public bool TryGetSessionByTelegramId(int id, out ISession session)
        {
            bool chatIsFound = BotClient.GetChatAsync(id).ContinueWith((chatTask) => chatTask.IsCompletedSuccessfully).Result;

            if (chatIsFound)
                session = GetSessionByTelegramId(id);
            else
                session = null;

            return chatIsFound;
        }

        public ISession GetSessionByTelegramId(int id)
        {
            var session = sessionsDictionary.GetOrAdd(id, new Session(MarkupTree.Root, id, this));

            if (BotOwner != null && BotOwner.Session == null && BotOwner.id == id)
            {
                BotOwner.Session = session;
            }
            return session;
        }
    }

    /// <summary>
    /// Хранит список пользователей бота и кол-во сообщений.
    /// Если статистика инициализирована неправильно (Значения в 
    /// БД больше, чем значения в памяти), то оно должно поругаться в лог
    /// и занести в память значения из бд
    /// </summary>
    public class BotStatisticsForest
    {
        private HashSet<int> _usersTelegramIds;
        private long _numberOfMessages;

        public BotStatisticsForest(HashSet<int> usersTelegramIds, long numberOfMessages)
        {
            if (usersTelegramIds == null)
                usersTelegramIds = new HashSet<int>();
         

            if (usersTelegramIds.Count > numberOfMessages)
            {
                throw new Exception( $"Количество пользователей бота не может быть больше кол-ва сообщений" +
                    $" пользователей = {usersTelegramIds.Count}," +
                    $" сообщений = {numberOfMessages}");
            }

            _usersTelegramIds = usersTelegramIds;
            _numberOfMessages = numberOfMessages;
        }

        public BotStatisticsForest()
        {
            _usersTelegramIds = new HashSet<int>();
            _numberOfMessages = 0;
        }


        public bool TryExpandTheListOfUsers(HashSet<int> dbListOfUsers)
        {
            bool at_least_one_user_has_been_added = false;
            foreach (var userTelegramId in dbListOfUsers)
            {
                if (!_usersTelegramIds.Contains(userTelegramId))
                {
                    at_least_one_user_has_been_added = true;
                    _usersTelegramIds.Add(userTelegramId);
                }
            }

            return at_least_one_user_has_been_added;
        }
        public int GetNumberOfAllUsers()
        {
            return _usersTelegramIds.Count;
        }

        public List<int> GetNewUsersTelegramIds(HashSet<int> dbUsersTelegramIds)
        {
            List<int> newUsersTelegramIds = new List<int>();

            foreach (int userTelegramId in dbUsersTelegramIds)
            {
                if (!_usersTelegramIds.Contains(userTelegramId))
                {
                    throw new Exception($"Набор telegramId пользователей в памяти не содержит" +
                        $"telegramId={userTelegramId} из бд");
                }
            }

            //Поиск новых значений
            foreach (int userTelegramId in _usersTelegramIds)
            {
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
            get => _numberOfMessages;
            set => _numberOfMessages = value;
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
        HashSet<int> _blocketUsersIds = new HashSet<int>();

        public StupidBotAntispam(HashSet<int> blocketUsersIds)
        {
            this._blocketUsersIds = blocketUsersIds;
        }

        public StupidBotAntispam()
        {
            this._blocketUsersIds = new HashSet<int>();
        }

        public bool UserIsBlockedForThisBot(int userTelegramId)
        {
            return _blocketUsersIds.Contains(userTelegramId);
        }

        public void UpdateTheListOfBannedUsers(HashSet<int> dbBannedUsersTelegramIds)
        {
            foreach (var telegramId in _blocketUsersIds)
            {
                //Если в памяти есть забаненый пользователь, а в бд его нет (пользователя разбанили)
                if (!dbBannedUsersTelegramIds.Contains(telegramId))
                {
                    //залогировать это событие
                    Console.WriteLine("Из списка забаненых пользователей пропал" +
                        $"пользователь с Id={telegramId}");
                }
            }

            foreach (var telegramId in dbBannedUsersTelegramIds)
            {
                //Добавлен новый забаненый пользователь
                if (!_blocketUsersIds.Contains(telegramId))
                {
                    _blocketUsersIds.Add(telegramId);
                    Console.WriteLine("Добавлен новый забаненый пользователь");
                }
            }
            _blocketUsersIds = dbBannedUsersTelegramIds;
        }
    }
}



