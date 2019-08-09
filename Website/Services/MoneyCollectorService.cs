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


/*
 Требования к системе снятия денег
 1) Снимает деньги раз в день (полночь по гринвичу?)
 2) Если несколько таких сервисов запущено, то они не должны дважды проводить снятие денег

    Решение:
    1) Просто делаю сервис на сайте
    2) Делаю отдельный лог для снятия денег
    3) Перед снятием денег проверяю лог 
     
     
     */

namespace Website.Services
{
    public class MoneyCollectorService
    {
        private StupidLogger _logger;
        private DbContextWrapper _dbContextWrapper;
        private StupidBotForSalesBookkeeper _bookkeper;

        public MoneyCollectorService(
            IConfiguration configuration, 
            StupidLogger _logger,
            StupidBotForSalesBookkeeper _bookkeper)
        {
            this._logger = _logger;
            _dbContextWrapper = new DbContextWrapper(configuration);
            this._bookkeper = _bookkeper;
            CollectPeriodically(CancellationToken.None);

        }

        //Периодический запуск
        public async Task CollectPeriodically(CancellationToken cancellationToken)
        {            
                
            DateTime tomorrow_00_00 = DateTime.Now
              .AddHours(-DateTime.Now.Hour)
              .AddMinutes(-DateTime.Now.Minute)
              .AddSeconds(-DateTime.Now.Second);

            TimeSpan interval = tomorrow_00_00 - DateTime.Now;

            await Task.Delay(interval, cancellationToken);

            while (true)
            {
                await Collect();
                await Task.Delay(new TimeSpan(24, 0,0), cancellationToken);
            }
        }

        //Списание денег со всех аккаунтов
        private async Task Collect()
        {
            _logger.Log(LogLevelMyDich.INFO, Source.OTHER, "Старт списывания денег");

            ApplicationContext _contextDb = _dbContextWrapper
                .GetDbContext();

            var dt = DateTime.Now.AddDays(-1);
            var blrs = _contextDb
                .BotLaunchRecords
                .Where(_blr => _blr.StartTime >= dt)
                .Select(_blr=>_blr.BotId)
                .ToList();

            List<int> botIds = new List<int>();

            //Убрать дубли
            for (int i = 0; i < blrs.Count; i++)
            {
                if(!botIds.Contains(blrs[i]))
                {
                    botIds.Add(blrs[i]);
                }
            }

            BotForSalesPrice actualPrice =_contextDb
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

                var _bot = _contextDb
                        .Bots
                        .Find(botId);

                var account = _contextDb
                    .Accounts
                    .Find(_bot.OwnerId);


                //Цена адекватная
                if (priceInfo.SumToday > 0)
                {

                    //TODO разобраться с датой
                    WithdrawalLog existingTransaction = _contextDb
                        .WithdrawalLog
                        .Where(_wl =>
                            _wl.BotId == botId
                            && _wl.AccountId == account.Id
                            && _wl.DateTime > DateTime.Now.AddDays(-1)
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

                    //Нет начатых транзакций с этим ботом

                    //Записать, что начата транзакция
                    _contextDb.WithdrawalLog.Add(new WithdrawalLog()
                    {
                        BotId = botId,
                        AccountId =account.Id,
                        TransactionStatus = TransactionStatus.TRANSACTION_STARTED,
                        TransactionStatusString = TransactionStatus.TRANSACTION_STARTED.ToString(),
                        DateTime = DateTime.Today.AddDays(-1)
                    });

                    _contextDb.SaveChanges();

                    //если у аккаунта есть деньги
                    if (account.Money > 0)
                    {
                        //списать деньги
                        account.Money -= priceInfo.SumToday;
                    }
                    else
                    {
                        //остановить всех ботов аккаунта
                        //выбрать всех работающих ботов
                        List<RouteRecord> rrs = _contextDb
                            .RouteRecords
                            .Join(_contextDb.Bots,
                                _rr=>_rr.BotId ,
                                __bot=>_bot.Id,
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

                                }

                            }
                            else
                            {
                                _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Join по RouteRecords не удался");
                            }
                        }
                    }

                    
                    _contextDb.SaveChanges();

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
    }
}
