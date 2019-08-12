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


            ApplicationContext context = _dbContextWrapper.GetNewDbContext();



            context.SaveChanges();
            _logger.Log(LogLevelMyDich.INFO,
                Source.FOREST,
                "Окончание синхронизации забаненных пользователей ");

        }
    }
}
