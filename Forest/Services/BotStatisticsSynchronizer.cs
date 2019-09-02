﻿using DataLayer;
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
    public class BotStatisticsSynchronizer
    {
        private DbContextWrapper _dbContextWrapper;
        private StupidLogger _logger;

        public BotStatisticsSynchronizer(IConfiguration configuration, StupidLogger logger)
        {
            _dbContextWrapper = new DbContextWrapper(configuration);
            _logger = logger;

            _logger.Log(LogLevelMyDich.INFO,
                Source.BOT_STATISTICS_SYNCHRONIZER,
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
                //        Source.BOT_STATISTICS_SYNCHRONIZER,
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

                //int five_minutes = 5 * 60*1000;
                int five_seconds = 5 *1000;
                //await Task.Delay(1000 );
                Thread.Sleep(five_seconds);
            }
        }

        //TODO Эта хрень не потокобезопасна
        private void SyncBotData()
        {
            _logger.Log(LogLevelMyDich.INFO,
                Source.BOT_STATISTICS_SYNCHRONIZER,
                "Старт обновления статистики ботов");

                         
            ApplicationContext context = _dbContextWrapper.GetNewDbContext();


            List<BotForSalesStatistics> allStatistics = context
                .BotForSalesStatistics
                .ToList();
             

            List<Record_BotUsername_UserTelegramId> allBotsUsers = context
                .BotUsers
                .ToList();
             

            //Для всех ботов в этом лесу актуальные данные кол-ва сообщений 
            //и данные о пользователях переносит в БД
             foreach (var botUsername in BotsContainer.BotsDictionary.Keys)
             {
                BotWrapper botWrapper = null;
                BotsContainer.BotsDictionary.TryGetValue(botUsername, out botWrapper);
                
                if (botWrapper == null)
                {
                    _logger.Log(
                        LogLevelMyDich.WARNING,
                        Source.BOT_STATISTICS_SYNCHRONIZER,
                        $"При обновлении статистики не удалось получить достпук к " +
                        $"боту botUsername={botUsername} в статическом контейнере");
                    continue;
                }

                #region Обновление кол-ва сообщений в бд 
                //запись статистики бота из бд
                BotForSalesStatistics statisticsDb = allStatistics
                    .Where(_stat => _stat.BotId == botWrapper.BotID)
                    .SingleOrDefault();
                 

                if (statisticsDb == null)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.BOT_STATISTICS_SYNCHRONIZER,
                        $"В бд нет статистики для бота, который запущен в лесу." +
                        $"botUsername={botUsername}, botWrapper.BotID{botWrapper.BotID}");
                    continue;
                }
                 
                //кол-во сообщений из памяти
                long actualNumberOfMessages = botWrapper.StatisticsContainer.NumberOfMessages;

                if (actualNumberOfMessages < statisticsDb.NumberOfUniqueMessages)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.BOT_STATISTICS_SYNCHRONIZER,
                        $"Обновление статистики бота в бд. Количество сообщений у бота в памяти " +
                        $"меньше старого значения кол-ва сообщений у бота в БД." +
                        $"actualNumberOfMessages{actualNumberOfMessages}, " +
                        $"statisticsDb.NumberOfUniqueMessages = {statisticsDb.NumberOfUniqueMessages}");

                    //Это может произойти, если при старте бота из бд не была извлечена статистика бота,
                    //которая накопилась за прошлые запуски

                    //Заношу в память данные из бд, чтобы такой ошибки больше не было
                    actualNumberOfMessages = statisticsDb.NumberOfUniqueMessages;
                }
                else
                {
                    //Обновление кол-ва сообщений
                    statisticsDb.NumberOfUniqueMessages = actualNumberOfMessages;
                }

                #endregion


                #region Обновление списка пользователей в бд
                var dbBotUsers = allBotsUsers
                    .Where(_record => _record.BotUsername == botUsername)
                    .Select(_record=>_record.BotUserTelegramId)
                    .ToHashSet();

                List<int> newUsersTelegramIds = null;

                //Упадёт, если в памяти не будет хотя бы одного id из БД
                try
                {
                    newUsersTelegramIds = botWrapper
                        .StatisticsContainer
                        .GetNewUsersTelegramIds(dbBotUsers);

                }
                catch(Exception ee)
                {
                    _logger.Log(LogLevelMyDich.ERROR,
                        Source.BOT_STATISTICS_SYNCHRONIZER,
                        "При обновлении списка пользователей произошла ошибка", ex:ee);
                }

                if (newUsersTelegramIds == null)
                {
                    //Не удалось нормально извлечь новых пользователей
                    //Обновить память для соответствия в бд и попробовать снова
                    bool success = botWrapper.StatisticsContainer.TryExpandTheListOfUsers(dbBotUsers);
                    if (!success)
                    {
                        try
                        {
                            newUsersTelegramIds = botWrapper
                              .StatisticsContainer
                              .GetNewUsersTelegramIds(dbBotUsers);
                        }catch(Exception eee)
                        {
                            _logger.Log(LogLevelMyDich.ERROR,
                                Source.BOT_STATISTICS_SYNCHRONIZER,
                                "При повторной попытке синхронизировать кол-во пользователей было брошено исключение",
                                ex: eee);
                            continue;
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevelMyDich.ERROR,
                            Source.BOT_STATISTICS_SYNCHRONIZER,
                            "Попытка добавить недостающих пользователей не увенчалась успехом.");
                        continue;
                    }
                }



                int actualNumberOfUsers = botWrapper.StatisticsContainer.GetNumberOfAllUsers();

                if (dbBotUsers.Count > actualNumberOfUsers)
                {
                    _logger.Log(LogLevelMyDich.WARNING,
                        Source.BOT_STATISTICS_SYNCHRONIZER,
                        $"Обновление статистики бота. У бота " +
                        $"botUsername {botUsername} botWrapper.BotID={botWrapper.BotID}" +
                        $"старое количество пользователей в БД больше актуального кол-ва " +
                        $"пользователей");
                }

                //Обновление кол-ва пользователей
                var botStat = context.BotForSalesStatistics
                    .Find(botWrapper.BotID);

                botStat.NumberOfUniqueUsers = actualNumberOfUsers;

                //Обновление списка пользователей бота
                List<Record_BotUsername_UserTelegramId> newUsersRecords = 
                    new List<Record_BotUsername_UserTelegramId>();

                for (int i = 0; i < newUsersTelegramIds.Count; i++)
                {
                    int newUserTelegramId = newUsersTelegramIds[i];
                    newUsersRecords.Add(new Record_BotUsername_UserTelegramId()
                    {
                        BotUsername = botUsername,
                        BotUserTelegramId = newUserTelegramId
                    });
                }

                context.BotUsers.AddRange(newUsersRecords);

                _logger.Log(LogLevelMyDich.INFO,
                    Source.BOT_STATISTICS_SYNCHRONIZER,
                    $"При обновлении статистики к списку пользователей добавлено " +
                    $"list.Count={newUsersRecords.Count} новых пользователей");

                #endregion

            }


            context.SaveChanges();



            _logger.Log(LogLevelMyDich.INFO,
                Source.BOT_STATISTICS_SYNCHRONIZER,
                "Окончание обновления статистики ботов");

        }

    }
}
