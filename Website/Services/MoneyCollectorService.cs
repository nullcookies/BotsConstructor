using DataLayer;
using DataLayer.Models;
using DataLayer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Website.Services.Bookkeeper;


namespace Website.Services
{
    public class MoneyCollectorService
    {
        private StupidLogger _logger;
        private DbContextWrapper _dbContextWrapper;
        private StupidBotForSalesBookkeeper _bookkeper;
        private BotsAirstripService _botsAirstripService;

        public MoneyCollectorService(
            StupidLogger _logger,
            IConfiguration configuration, 
            StupidBotForSalesBookkeeper _bookkeper,
            BotsAirstripService botsAirstripService)
        {
            this._logger = _logger;
            this._bookkeper = _bookkeper;
            this._botsAirstripService = botsAirstripService;
            _dbContextWrapper = new DbContextWrapper(configuration);
            
            CollectPeriodically(CancellationToken.None);

        }

        public void Start() { }
        //Периодический запуск
        public async Task CollectPeriodically(CancellationToken cancellationToken)
        {            
                
            DateTime tomorrow_00_00 = DateTime.Now
              .AddHours(    -DateTime.Now.Hour)
              .AddMinutes(  -DateTime.Now.Minute)
              .AddSeconds(  -DateTime.Now.Second);

            TimeSpan interval = tomorrow_00_00 - DateTime.Now;

            //await Task.Delay(interval, cancellationToken);

            while (true)
            {
                Collect();
                await Task.Delay(new TimeSpan(24, 0,0), cancellationToken);
            }
        }

        //Списание денег со всех аккаунтов
        private void Collect()
        {
            _logger.Log(LogLevelMyDich.INFO, Source.OTHER, "Старт списывания денег");

            ApplicationContext contextDb = _dbContextWrapper
                .GetDbContext();

            var dt = DateTime.Now.AddDays(-1);
            var blrs = contextDb
                .BotLaunchRecords
                .Where(_blr => _blr.StartTime >= dt)
                .Select(_blr=>_blr.BotId)
                .ToList();

            //Боты, которые запускались сегодня (с повторами)
            List<int> botIds = new List<int>();

            //Убрать дубли
            for (int i = 0; i < blrs.Count; i++)
            {
                if(!botIds.Contains(blrs[i]))
                {
                    botIds.Add(blrs[i]);
                }
            }

            //Боты, которые запускались сегодня (уникальные)
            BotForSalesPrice actualPrice =contextDb
                .BotForSalesPrices
                .Last();

            if (actualPrice == null)
            {
                _logger.Log(LogLevelMyDich.FATAL, Source.OTHER, "Нет тарифа в бд! аааааа");
                return;
            }

            //TODO все боты считаются ботами для продаж


            //Все боты, которые сегодня работали
            for (int i = 0; i < botIds.Count; i++)
            {
                int botId = botIds[i];
             
                var priceInfo = _bookkeper.GetPriceInfo(botId);

                var bot = contextDb
                        .Bots
                        .Find(botId);

                var account = contextDb
                        .Accounts
                        .Find(bot.OwnerId);

                

                //Цена за день адекватная
                if (priceInfo.SumToday > 0)
                {
                    //Транзакции снятия денег с аккаунта за этого бота сегодня уже были?
                    //Может возникнуть, если запущено несколько сервисов списывания денег

                    WithdrawalLog existingTransaction = contextDb
                        .WithdrawalLog
                        .Where(_wl =>
                            _wl.BotId == botId
                            && _wl.AccountId == account.Id
                            && _wl.DateTime == new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                            && _wl.TransactionStatus==TransactionStatus.TRANSACTION_STARTED
                        ).SingleOrDefault() ;


                    if (existingTransaction != null)
                    {
                        switch (existingTransaction.TransactionStatus)
                        {   
                            case TransactionStatus.TRANSACTION_STARTED:
                                //отложить задачу 
                                break;
                            case TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL:

                                _logger.Log(LogLevelMyDich.ERROR, Source.MONEY_COLLECTOR_SERVICE, "Запущено несколько сервисов списывания денег");
                                //не делать ничего
                                continue;
                            case TransactionStatus.TRANSACTION_FAILED:
                                //произошло дерьмо
                                break;
                            default:
                                //упасть с фатальной ошибкой
                                _logger.Log(
                                    LogLevelMyDich.FATAL,
                                    Source.MONEY_COLLECTOR_SERVICE,
                                    "Неожиданный статус транзакции");
                                return;
                        }

                    }

                    //Нет начатых транзакций с этим (бот, аккаунт, день)

                    //Записать, что начата транзакция
                    contextDb.WithdrawalLog.Add(new WithdrawalLog()
                    {
                        BotId = botId,
                        AccountId =account.Id,
                        TransactionStatus = TransactionStatus.TRANSACTION_STARTED,
                        TransactionStatusString = TransactionStatus.TRANSACTION_STARTED.ToString(),
                        DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                    });

                    contextDb.SaveChanges();

                    //если у аккаунта есть деньги
                    if (account.Money > 0)
                    {
                        //списать деньги
                        account.Money -= priceInfo.SumToday;
                    }
                    else
                    {
                        //остановить всех ботов, которые принадлежат обанкротившемуся аккаунту
                        List<RouteRecord> rrs = contextDb
                            .RouteRecords
                            .Join(contextDb.Bots,
                                _rr=>_rr.BotId ,
                                __bot=>bot.Id,
                                (_rr, __bot) => new RouteRecord
                                {
                                  BotId = _rr.BotId,
                                  ForestLink = _rr.ForestLink,
                                  Bot = __bot
                                }).ToList();

                        for (int q = 0; q < rrs.Count; q++)
                        {
                            if (rrs[q].Bot != null)
                            {
                                bool bot_belongs_to_the_desired_account = rrs[q].Bot.OwnerId == account.Id;

                                //бот принадлежит обанкротившемуся аккаунту
                                if (bot_belongs_to_the_desired_account)
                                {
                                    //остановить
                                    _botsAirstripService.StopBot(rrs[q].Bot.Id, account.Id);

                                }

                            }
                            else
                            {
                                _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Join по RouteRecords не удался");
                            }
                        }
                    }

                    //Создать запись о удачной транзакции
                    WithdrawalLog withdrawalLog = contextDb
                        .WithdrawalLog
                        .Where(_wl => _wl.AccountId == account.Id
                        && _wl.BotId == bot.Id
                        && _wl.DateTime == GetTodayDate())
                        .Single();

                    if (withdrawalLog == null)
                    {
                        _logger.Log(LogLevelMyDich.FATAL,
                            Source.MONEY_COLLECTOR_SERVICE,
                            "При подтверждении успешной транзакции не была найдена запись о старте этой трензакции",
                            accountId: account.Id);
                        return;
                    }
                    withdrawalLog.TransactionStatus = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL;
                    withdrawalLog.TransactionStatusString = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL.ToString();

                    contextDb.SaveChanges();

                }
                else
                {
                    //Произошло дерьмо
                    _logger.Log(LogLevelMyDich.FATAL,
                        Source.OTHER,
                        $"Неадекватная цена. botId={botId} цена ={account.Money }");
                    return;
                }             
            }


            




        }

        private DateTime GetTodayDate()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }
    }
}
