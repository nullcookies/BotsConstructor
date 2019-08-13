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
        private DbContextWrapper _dbContextWrapper;
        private StupidLogger _logger;

        public BannedUsersSynchronizer(IConfiguration configuration, StupidLogger logger)
        {
            _dbContextWrapper = new DbContextWrapper(configuration);
            _logger = logger;

            _logger.Log(LogLevelMyDich.INFO,
                Source.FOREST,
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
                  //        Source.FOREST,
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
                Thread.Sleep(five_seconds);
            }
        }

        
        private void SyncBotData()
        {
            _logger.Log(LogLevelMyDich.INFO,
                Source.FOREST,
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
                        Source.FOREST,
                        "При синхронизации списка забаненных пользователей не удалось извлечь " +
                        "объект бота из статического словаря");
                    continue;
                }

                if (botWrapper == null)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.FOREST,
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
                Source.FOREST,
                "Окончание синхронизации забаненных пользователей ");

        }
    }
}
