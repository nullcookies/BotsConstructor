using DataLayer;
using DataLayer.Models;
using DataLayer.Services;
using DeleteMeWebhook;
using LogicalCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Forest.Services
{
    public class BannedUsersSynchronizer
    {
        private DbContextFactory _dbContextWrapper;
        private StupidLogger _logger;

        public BannedUsersSynchronizer(IConfiguration configuration, StupidLogger logger)
        {
            _dbContextWrapper = new DbContextFactory(configuration);
            _logger = logger;

            _logger.Log(LogLevelMyDich.INFO,
                Source.BANNED_USERS_SYNCHRONIZER,
                "Конструктор синхронизатора бд");

            (new Thread(
              () =>
              {
                  //try
                  //{
                  RunSyncDbBots();
                  //}
                  //catch (Exception ee)
                  //{
                  //    _logger.Log(LogLevelMyDich.ERROR,
                  //        Source.BANNED_USERS_SYNCHRONIZER,
                  //        "Упал сервис синхронизации статистики ботов", ex: ee);
                  //}
              }
              )).Start();

        }

        private void RunSyncDbBots()
        {
            while (true)
            {
                SyncBotData();

                
                int five_seconds = 5 * 1000;
                Thread.Sleep(five_seconds*10);
            }
        }

        
        private void SyncBotData()
        {
            _logger.Log(LogLevelMyDich.INFO,
                Source.BANNED_USERS_SYNCHRONIZER,
                "Старт синхронизации забаненных пользователей ");


            ApplicationContext contextDb = _dbContextWrapper.GetNewDbContext();
            //Записать в память новых забаненных пользователей, если они есть
            int countOfBptsInThisForest = BotsContainer.BotsDictionary.Count;
            foreach (string botUsername in BotsContainer.BotsDictionary.Keys)
            {
                BotWrapper botWrapper = null;
                bool success = BotsContainer.BotsDictionary.TryGetValue(botUsername, out botWrapper);
                if (!success)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.BANNED_USERS_SYNCHRONIZER,
                        "При синхронизации списка забаненных пользователей не удалось извлечь " +
                        "объект бота из статического словаря");
                    continue;
                }

                if (botWrapper == null)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.BANNED_USERS_SYNCHRONIZER,
                        "При синхронизаци спсика забаненных пользователей из котейнера был извлечён" +
                        "бот = null");
                }

                HashSet<int> bannedUsers = contextDb
                    .BannedUsers
                    .Where(_bu => _bu.BotUsername == botUsername)
                    .Select(_bu=>_bu.UserTelegramId)
                    .ToHashSet();

                botWrapper.StupidBotAntispam
                    .UpdateTheListOfBannedUsers(bannedUsers);

            }


            _logger.Log(LogLevelMyDich.INFO,
                Source.BANNED_USERS_SYNCHRONIZER,
                "Окончание синхронизации забаненных пользователей ");

        }
    }
}
