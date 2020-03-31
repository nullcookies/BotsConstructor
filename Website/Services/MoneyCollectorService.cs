//using DataLayer;
//using DataLayer.Models;
//using DataLayer.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Website.Services.Bookkeeper;


//namespace Website.Services
//{
//    public class MoneyCollectorService
//    {
//        private StupidLogger _logger;
//        private DbContextWrapper _dbContextWrapper;
//        private StupidBotForSalesBookkeeper _bookkeper;
//        private BotsAirstripService _botsAirstripService;
//        private IServiceScopeFactory serviceScopeFactory;

//        public MoneyCollectorService(
//            StupidLogger _logger,
//            IConfiguration configuration,
//            StupidBotForSalesBookkeeper _bookkeper,
//            BotsAirstripService botsAirstripService,
//            IServiceScopeFactory serviceScopeFactory)
//        {
//            this._logger = _logger;
//            this._bookkeper = _bookkeper;
//            this._botsAirstripService = botsAirstripService;
//            _dbContextWrapper = new DbContextWrapper(configuration);
//            this.serviceScopeFactory = serviceScopeFactory;
//            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Констуктор сервиса сбора денег.");

//            (new Thread(
//                () =>
//                {
//                    CollectPeriodically();
//                }
//            )).Start();         
//        }

        

//        /// <summary>
//        /// При запуске ждёт до 00 05 по UTC и запускает метод снятия денег с аккаунтов
//        /// </summary>
//        public void CollectPeriodically()
//        {
            
//            _logger.Log(LogLevelMyDich.IMPORTANT_INFO, Source.MONEY_COLLECTOR_SERVICE, "Запуск сервиса для сбора денег");

//            //Задержка до 5 минут первого часа ночи
//            DateTime tomorrow_00_05_00 = DateTime.UtcNow
//              .AddDays(1)
//              .AddHours(-DateTime.UtcNow.Hour)
//              .AddMinutes(-DateTime.UtcNow.Minute + 5)
//              .AddSeconds(-DateTime.UtcNow.Second);

//            TimeSpan interval = tomorrow_00_05_00 - DateTime.UtcNow;

//            _logger.Log(LogLevelMyDich.IMPORTANT_INFO, Source.MONEY_COLLECTOR_SERVICE, $"До первого сбора денег осталось {interval}");

//            //Ждёмс
//            Thread.Sleep(interval);
//            //await Task.Delay(interval);


//            try
//            {
//                Collect();
//            }
//            catch (Exception eee)
//            {
//                _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Сервис списывания денег упал", ex: eee);
//                Console.WriteLine(eee.Message);
//            }

//            CollectPeriodically();

//        }



//        //Списание денег со всех аккаунтов
//        //за ботов, которые работали вчера.
//        //Например: запуск происходит в 00 05 / 13 08 2019
//        //Тогда метод должен списать деньги за ботов, которые запускались 
//        // с 00 00 / 12 08 2019 до 23 59 /12 08 2019
//        private void Collect()
//        {

//            var contextDb = _dbContextWrapper.CreateTestDbContext();

//            var yesterday_00_00 = GetTodayDate().AddDays(-1);
//            var today_00_00 = GetTodayDate();


//            //Все боты, которые работали за вчера
//            List<int> idsOfTheBotsThatWorkedYesterday = contextDb.BotWorkLogs
//                .Where(_bl => _bl.InspectionTime > yesterday_00_00
//                    && _bl.InspectionTime < today_00_00)
//                .Select(_bl => _bl.BotId)
//                .ToList();


//            //Убрать дубликаты
//            idsOfTheBotsThatWorkedYesterday = idsOfTheBotsThatWorkedYesterday
//                .GroupBy(x => x)
//                .Select(x => x.First()).ToList();

//            var botIds = idsOfTheBotsThatWorkedYesterday;

//            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Количество ботов, которые вчера запускались = " + botIds.Count);

//            //Все боты, которые вчера работали
//            for (int i = 0; i < botIds.Count; i++)
//            {
//                int botId = botIds[i];                 

//                BotDB bot = null;
//                Account account = null;
//                StupidPriceInfo priceInfo = null;

//                bot = contextDb.Bots.Find(botId);
//                priceInfo = _bookkeper.GetPriceInfo(botId, DateTime.Today.AddDays(-1));
//                account = contextDb.Accounts.Find(bot.OwnerId);
                 
//                //Цена за день адекватная
//                if (priceInfo.SumToday > 0)
//                {
//                    //Транзакции снятия денег с аккаунта за этого бота сегодня уже были?
//                    //Может возникнуть, если запущено несколько сервисов списывания денег

//                    WithdrawalLog existingTransaction = contextDb
//                        .WithdrawalLog
//                        .Where(_wl =>_wl.BotId == botId
//                            && _wl.DateTime == today_00_00)
//                        .SingleOrDefault();
                     

//                    //если с ботом есть транзакция 
//                    if (existingTransaction != null)
//                    {
//                        _logger.Log(LogLevelMyDich.WARNING,
//                            Source.MONEY_COLLECTOR_SERVICE,
//                            $"existingTransaction != null, existingTransaction.Status={existingTransaction.TransactionStatus}");

                         

//                        switch (existingTransaction.TransactionStatus)
//                        {
//                            //Другой сервис снимает деньги
//                            case TransactionStatus.TRANSACTION_STARTED:
//                                //TODO отложить задачу 
//                                break;
//                            //Другой сервис снял деньги
//                            case TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL:
//                                _logger.Log(LogLevelMyDich.ERROR, Source.MONEY_COLLECTOR_SERVICE, "Запущено несколько сервисов списывания денег");
//                                //не делать ничего
//                                continue;
//                            default:
//                                //упасть с фатальной ошибкой
//                                _logger.Log(
//                                    LogLevelMyDich.FATAL,
//                                    Source.MONEY_COLLECTOR_SERVICE,
//                                    "Неожиданный статус транзакции");
//                                throw new Exception("Неожиданный статус транзакции");
//                        }

//                    }
                     

//                    //Нет начатых транзакций с этим (бот, день)
//                    //Записать, что начата транзакция
//                    contextDb.WithdrawalLog.Add(new WithdrawalLog()
//                    {
//                        BotId = botId,
//                        AccountId = account.Id,
//                        TransactionStatus = TransactionStatus.TRANSACTION_STARTED,
//                        TransactionStatusString = TransactionStatus.TRANSACTION_STARTED.ToString(),
//                        DateTime = today_00_00
//                    });
                     

//                    contextDb.SaveChanges();


//                    _logger.Log(LogLevelMyDich.INFO,
//                        Source.MONEY_COLLECTOR_SERVICE,
//                        $"account.Money = {account.Money}");
                     


//                    //если у аккаунта есть деньги
//                    if (account.Money >= 0)
//                    {
//                        if (priceInfo.SumToday >= 0)
//                        {
//                            //списать деньги
//                            account.Money -= priceInfo.SumToday;

//                            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, 
//                                $"После списывания на аккаунте осталось {account.Money}", accountId: account.Id);
//                        }
//                        else
//                        {
//                            _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Цена бота в день отрицательна!");
//                            throw new Exception("Цена бота в день отрицательна!");
//                        }
//                    }
//                    else
//                    {
//                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"На аккаунте нет денег. Остановка всех ботов", accountId: account.Id);

//                        StopAllAccountBots(contextDb, account);
//                    }
                    
//                    //Создать запись о удачной транзакции
//                    WithdrawalLog withdrawalLog = contextDb
//                        .WithdrawalLog
//                        .Where(_wl =>_wl.BotId == bot.Id
//                            && _wl.DateTime == today_00_00)
//                        .Single();

//                    if (withdrawalLog == null)
//                    {
//                        _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE,
//                            "При подтверждении успешной транзакции не была найдена запись о старте этой транзакции",
//                            accountId: account.Id);

//                        throw new Exception("При подтверждении успешной транзакции не была найдена запись о старте этой транзакции");

//                    }

//                    withdrawalLog.TransactionStatus = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL;
//                    withdrawalLog.TransactionStatusString = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL.ToString();
//                    withdrawalLog.Price = priceInfo.SumToday;

//                    contextDb.SaveChanges();

//                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Транзакция прошла успешно", accountId:account.Id);
//                }
//                else
//                {
//                    //Произошло дерьмо
//                    _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE,
//                        $"Неадекватная цена. botId={botId} цена ={account.Money }");

//                    throw new Exception("Неадекватная цена");
//                }
//            }
//        }

//        /// <summary>
//        /// Today 00:00
//        /// </summary>
//        /// <returns></returns>
//        public static DateTime GetTodayDate()
//        {
//            return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
//        }

//        /// <summary>
//        /// Останавливает всех ботов аккаунта
//        /// </summary>
//        /// <param name="contextDb"></param>
//        /// <param name="account"></param>
//        private void StopAllAccountBots(ApplicationContext contextDb, Account account)
//        {

//            List<RouteRecord> rrs = contextDb
//                .RouteRecords
//                .Join(contextDb.Bots,
//                    _rr => _rr.BotId,
//                    _bot => _bot.Id,
//                    (_rr, __bot) => new RouteRecord
//                    {
//                        BotId = _rr.BotId,
//                        ForestLink = _rr.ForestLink,
//                        Bot = __bot
//                    }).ToList();

//            //По всем ботам этого аккаунта
//            for (int q = 0; q < rrs.Count; q++)
//            {
//                BotDB _bot = rrs[q].Bot;

//                _logger.Log(LogLevelMyDich.INFO,
//                    Source.MONEY_COLLECTOR_SERVICE,
//                    $"Остановка всех ботов аккаунта. _bot.Id = {_bot?.Id}");

//                if (_bot != null)
//                {
//                    bool bot_belongs_to_the_desired_account = rrs[q].Bot.OwnerId == account.Id;

//                    //бот принадлежит аккаунту
//                    if (bot_belongs_to_the_desired_account)
//                    {
//                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE,
//                            $"Вызов остановки бота bot.Id= {_bot.Id}, _bot.OwnerId={_bot.OwnerId} ");
                        

//                        //остановить
//                        JObject result = _botsAirstripService.StopBot(rrs[q].Bot.Id, account.Id);
//                        if (!(bool)result["success"])
//                        {
//                            _logger.Log(
//                                LogLevelMyDich.ERROR,
//                                Source.MONEY_COLLECTOR_SERVICE,
//                                $"Не удалось остановить бота bot.Id= {_bot.Id}, _bot.OwnerId={_bot.OwnerId} failMessage = {result["failMessage"]}");
//                        }
//                    }
//                }
//                else
//                {
//                    _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Join по RouteRecords не удался");
//                }
//            }
//        }
//    }
//}
