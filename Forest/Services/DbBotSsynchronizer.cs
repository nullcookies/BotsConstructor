using DataLayer;
using DataLayer.Models;
using DataLayer.Services;
using DeleteMeWebhook;
using LogicalCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Forest.Services
{
    public class BotStatisticsSynchronizer
    {
        private DbContextWrapper _dbContextWrapper;
        private StupidLogger _logger;

        public BotStatisticsSynchronizer(DbContextWrapper dbContextWrapper)
        {
            _dbContextWrapper = dbContextWrapper;

            (new Thread(
              () =>
              {
                  RunSyncDbBots().Wait();
              }
              )).Start();

        }

        private async Task RunSyncDbBots()
        {

            while (true)
            {
                SyncBotData();
                //Через каждые 5 минут
                await Task.Delay(1000 * 60 * 5);
            }
        }

        private void SyncBotData()
        {
            ApplicationContext context = _dbContextWrapper.GetNewDbContext();

            //List<BotForSalesStatistics> allDbStatistics = context
            //    .BotForSalesStatistics
            //    .Join(context.Bots,
            //    _stat=>_stat.BotId,
            //    _bot=>_bot.Id,
            //    (_stat, _bot)=>new BotForSalesStatistics
            //    {
            //        Bot = _bot,
            //        NumberOfOrders =_stat.NumberOfOrders,
            //        NumberOfUniqueMessages = _stat.NumberOfUniqueMessages,
            //        NumberOfUniqueUsers = _stat.NumberOfUniqueUsers,
            //        BotId = _stat.BotId
            //    }).ToList();

            List<BotForSalesStatistics> allStatistics = context
                .BotForSalesStatistics
                .ToList();

            List<Record_BotUsername_UserTelegramId> allBotsUsers = context
                .BotUsers
                .ToList();

            foreach (var botUsername in BotsContainer.BotsDictionary.Keys)
            {
                BotWrapper botWrapper = null;
                BotsContainer.BotsDictionary.TryGetValue(botUsername, out botWrapper);
                if (botWrapper == null)
                {
                    _logger.Log(
                        LogLevelMyDich.WARNING,
                        Source.FOREST,
                        $"При обновлении статистики не удалось получить достпук к " +
                        $"боту botUsername={botUsername} в статическом контейнере");
                }

                BotForSalesStatistics statisticsDb = allStatistics
                    .Where(_stat => _stat.BotId == botWrapper.BotID)
                    .SingleOrDefault();

                if (statisticsDb == null)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.FOREST,
                        $"В бд нет статистики для бота, который запущен в лесу." +
                        $"botUsername={botUsername}, botWrapper.BotID{botWrapper.BotID}");
                    continue;
                }

                int actualNumberOfMessages = botWrapper.StatisticsContainer.NumberOfMessages;
                if (actualNumberOfMessages < statisticsDb.NumberOfUniqueMessages)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.FOREST,
                        $"Обновление статистики бота в бд. Количество сообщений у бота в памяти " +
                        $"меньше старого значения кол-ва сообщений у бота в БД." +
                        $"actualNumberOfMessages{actualNumberOfMessages}, " +
                        $"statisticsDb.NumberOfUniqueMessages = {statisticsDb.NumberOfUniqueMessages}");
                }
                statisticsDb.NumberOfUniqueMessages = actualNumberOfMessages;


                List<int> memorybotUsers = botWrapper.StatisticsContainer.usersTelegramIds;

                List<int> dbBotUsers = allBotsUsers
                    .Where(_record => _record.BotUsername == botUsername)
                    .Select(_record=>_record.BotUserTelegramId)
                    .ToList();

                if (dbBotUsers.Count > memorybotUsers.Count)
                {
                    _logger.Log(LogLevelMyDich.WARNING,
                        Source.FOREST,
                        $"Обновление статистики бота. У бота " +
                        $"botUsername {botUsername} botWrapper.BotID={botWrapper.BotID}" +
                        $"старое количество пользователей в БД больше кол-ва пользователей в БД");
                }

                dbBotUsers.AddRange(memorybotUsers);

                //Магия, которая убирает дубликаты
                dbBotUsers.GroupBy(x => x).Select(x => x.First());


            }


            context.SaveChanges();
            //Для всех ботов в лесу
            //Записать новую статистику бота в бд
            //Обновить список спамеров для бота
        }
    }
}
