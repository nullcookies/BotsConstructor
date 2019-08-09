using DataLayer;
using DataLayer.Models;
using DataLayer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Website.Services.Bookkeeper;

/*
 Эта херня периодически падает с 

    System.ObjectDisposedException: Cannot access a disposed object. A common cause of this error is disposing a context that was resolved from dependency injection and then later trying to use the same context instance elsewhere in your application.
This may occur is you are calling Dispose() on the context, or wrapping the context in a using statement.
If you are using dependency injection, you should let the dependency injection container take care of disposing context instances.

    Это значит, что объект контекста используется в разных потоках.
    Один поток заканчивает, убивает контекст, а другой ловит ошибку

    Или где-то есть async void 
    нужно заменить на async Task


    
     */


//    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@&&&###############((((((((((((((((((((((((((((((((((((((((((((((((**/(#%%&&@@@@@@@@###%#**../////////////////////////////********//////////////////////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@@&%(#############((#((((((((((((((((((((((((((((((((((((((((((((/*/#%&&%%%&@@@@@@@@#/*/#(//,,*///////////////////*****************////////////////////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@@&&####################((((((((((((((((((((((((((((((((((((((((/(####%%%%%&&&&@@@@@@@&&%%%#(/*,/////////////******************************////////////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@@@@%####################(((#(((((((((((((((((((((((((((((((((((/%####%%%%&&&&&&&&@@@@@&&@&&&%(/,*/////////***********************************/////////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&####################(((#((((((((((((((((((((((((((((((((((((######%%%&&&&&&@@@@@@@@@@@@&&%#/,*//////****************************************//////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%########################(((((((((((((((((((((((((((((((((((######%%%&&&&@@@@@@@@@@@@@@@&%%#/**/*********************************************/////////////////
//@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%########################(((((((((((((((((((((((((((((((((((#######%%&&&&@@@@@@&@@@@@@@@@@&%(/***************,,,,,,,,,,*************************//////////////
//&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&##########################(((((((((((((((((((((((((((((((((((###%%&&%%&&&&&&&&&&&&@@@@@@@@&#/************,,,,,,,,,,,,,,,,,,********************//////////////
//&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#######################(((((((((((((((((((((((((((((((((#%%###%@@&%%%&@@&&&@@&&&&&&@@@@@@&&#/*********,,,,,,,,,,,,,,,,,,,,,,,,,*****************////////////
//&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#############################((((((((((((((((((((((((((((##%%%@@@@@&%&&&&&&@@&&&&&&@@@@@@&@&/********,,,,,,,,,....,,,,,,,,,,,,,,,,,*************////////////
//&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#################################(((((((((((((((((((((((((%%%@@@@@&&&&&&&&&@@&&&&&&@@@@@@@@&#/******,,,,,,,,...........,,,,,,,,,,,,,,*************//////////
//&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&##############################(#((((((#(((((((((((((((((((##%@@@@@@&%&&@@@@@@&&&&&@@@@@@@@@@#/*****,,,,,,,................,,,,,,,,,,,*************//////////
//&&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#######################((####%((#%&((%&%**((((((((((((((((#%%&@@@@@&&%&&@@@@@@@&&@@@@@@@@@&&#(/*,*,,,,,,,,...................,,,,,,,,,*************/////////
//&&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&#####################(%&&@@@&%%#/#&&%%/(,,(#/(((((((((((((###%%&&@@@@&&&@@@@@@@@@@@@@@@@&&%%#((*,,,,,,,,,....................,,,,,,,,,*************/////////
//&&&&&&&@@&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@&%#################%&&&@&@@@@@@@@@@&#/,.#*(&@@@&((((((((((((##%%%%&@@@@&&&@@@@@@@@@@@@@@@@&&&&%%//,,,,,,,,,......................,,,,,,,,***********//////////
//&&&&&&&&&@@@@&&@@@@@@@@@@@@@@@@@@@@@@@@&%################&@@@&&&@@@@@@&&&&###/*,,,*@@@&((((((((((((#%%&##%&@@@@@@@@@@@@@@@@@@@@@@@&&&&%%(,,,,,,,,,,.....................,,,,,,,,***********//////////
//&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@&%###############&&@@&&%%@@@&&@@@@@%(%(#((%%*/%%((#####((((((##%%&&@@@@@@@@@@@@@@@@@@@@@@@@@&&&&%#*.,,,,,,,,,,,**,,..............,,,,,,,************//////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@@&%##############&&&&&%%%#%&&&&@@@@@@&&&&%#(%%###(#########(((##%%&&@@@@@@@@@@@@@@@@@@@@@@@@@@@&&%(*,**,,*%&@&#,....,,...........,,,,,,,,***********///////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@@&%#############%&&&&&%%##%##%@@@@@@@@@@&&%%&@@@@&%%%#%%%####((%&@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%##(/**(%@@&/.      .,*,........,,,,,,,,,**********////////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@&%##############&&%%&%#####%&@@@@@@@@@@@@%%#&@@@@@@%#%&&&&&%#*@@@@@@@@@@@@@@@@@@@@@&@&&&&@@@@@&&%%&&&&&%@@@(..,**/#%%%####/,..,,,,,,,,,**********//////////////
//&&&&&&&&&&&&&&&@@&@@@@@@@@@@@@@@@@@@@@&%#############%&%%%&#####%%@&&@@@@@@@@@&&&&@@@@@@@@&&&@@@&&&@@@@@@@@@@@@@@@@@@@@@@@@@@&&&&&@@@@@@@@@&&@@%/((#(#%&&&@&&&&&%%%(,,,,,,,,**********///////////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@&((############%%%%%%((####%&&&&&&&&@@@@@&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&&&&@@@@@@@@@@&&&&&&&&&@@@@@@&&&&%%%(,,,**********//////////////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@&#(//##########%%&%&(((##%%&&&&@&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@@@@&@@@@@@@@@@@@@@&&&&&&@@@@@@@@@@@@@@&&@@@@@@@@@&&&&%%#**********////////////////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@&&##((/*(#######%&&&@#%%%%%(%%&@@@@@@@@@@&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&@@&&@@@@@@@@@@&&@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&&%%#******///////////////////////
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@&%#####((//#####%&&@@@@&&&&##%&@@@@@&&&&&&@@&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&&&&&@@@&&@@@@&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%#/**//////////////////////(//
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@&%#########(/(##%&&@@@@@@@&#(%&&@@@@&&&&&@&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@@@@@@@@@&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%#////////////////////(/(((((
//&&&&&&&&&&&&&&&&&&@@@@@@@@@@@&@@@@@@&%############(/(&&@@@@@@@&&#(%%&&@@@@&&&&&&@&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&@&&&@@@@&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%////////////////(((((((((((
//&&%&&&&&&&&&&&&&&&@@@@@@@@@@@&@@@@@@&################&@@@@@@@@&&%(#%&&@@@@@&&&&@@@@@@@@@@@@@@@@@@@@@@@&&@&@@@@@@@@@&&@@@@&&&&&&&&@@@&@&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&&%%(/////////////(((((((((((((
//%%%&&&&&&&&&&&&&&&@@@@@@@@@@&@@@@@@&&##############%&&@@@@@@@&&&&%&&%&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&@&@@@@@@@@&&@@@@&&&@&&&&&&@@@&&&&@&@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%((///(((((((((((((((((((((
//%%%%&&&&&&&&&&&&&&@@@@@@@@@@&@@@@@@&%###############&@@@&@@@&&&&&&&&&&&&&@@@@@@@@@@@@@@@@@@@@@@@@@@@&&@&&&&@@@@@@@&&@@@%&&&@&&%&&&%@@@@%%&%%&@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%(((((((((((((((((((((((((
//%%%%%%&&&&&&&&&&&&@@@@@@@@@@@@@@@@@&%###############&@@@@@@&&&&&&&&&&&&&&&@@@@@&@@@@@@@@@@@@@@@@@@@@&&&&&&&@@@@@@&&@@@%%&&@@&&&&&&&%%@@@&#@@&%%&@@@@@@@@@@@@@@@@@@@@@@&&&&%%%((((((((((((((((((((((((
//%%%%%%%&&&&&&&&&&&@@@@@@@@@&@@@@@@&%################@@@&&@@@&&&&&&&&&@@@&&&@@@@@@@@@@@@@@@@@@@@@@@@&&&&&&&&&@@@@&&@&@%&&@&@@&&&&&&&&%%@@@&#@@@&&&@@@@@@@@@@@@@@@@@@@@@&&&&%%%%(((((((((((((((((((((((
//%%%%%%%%&&&&&&&&&&@@@@@@@&&&@@@@@&&################%@@&%#%&&&&&&&@&@@@@&&&&&@@@@@@@@@@@@@@@@@@@@@@@&%&&&&&&&@@@@&&&&%%&%@&@@&&&&&&&%%%#@@@@#%@@@@&&@@@@@@@@@@@@@@@@@@@@@&&&%%%%((((((((((((((((((((((
//%%%%%%%%%&&&&&&&&&@@@@@@@&&@@@@@@&&################%&@%&%&&&&&&&&&&@&&%%&&&&@@@@@@@@@@@@@@@@@@@@@@@&%%&&&%&&@@@@@@%%%&%&@&@@&&&&&&&&%%##&@@&%#@@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%#(((((((((((((((((((((
//%%%%%%%%%%&&&&&&&@@@@@@@@&&@@@@&@&%################%&&%%&&&@&@&&&&@&&&&&&&&&&@@@@@@@@@@@@@@@@@&&@@&%%%&&&%&&&@@@@@%%%%%&@&@@@@&&&&&&&%%##&&@@%#@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%%#((((((((((((((((((((
//%%%%%%%%%%&&&&&&&@@@@@@@&&@@@@@@&&#################%&&&%&&@@@@@@&&&@&&&%%&&&&@@@@@@@@@@@@@@@@@&&@@&%%%%%&%&&&@@@&%%%%%&&@&@@@@&&&&&&&&%%#(%&@@&(@@@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%%#(((((((((((((((((((
//%%%%%%%%%%&&&&&&&@@@@@@@&@@@@@&&&%#################%%%&&@@@&&&@@@@&&&&&&&&&&@@@@@@@@@@@@@@@@@@@&&@&%%%&%&&&&&&@@#%%%%&&&@&@@&&&&&&&&%%%%%###&&&%(@@@@@@@@@@@@@@@@@@@@@@@@@@&%%%%%%#((((((((((((((((((
//%%%%%%%%%%&&&&&&&@@@@@&&&@@@@&@&&#####################&&&&%%%%&@@@@&&&&&&&&@@@@@@@@@@@@@@@@@@@&%&@&%%#%%&&&&&&@%%%%%%&&&@&@@&@&@&&%&%&%%%####%&&&#@@@@@@@@@@@@@@@@@@@@@@@@@@&%%%%%#((((((((((((((((((
//##%%%%%%%%%%&&&&&@@@@&&&@@@&&@@@&####################%%&&%%%%&&&@&@&&%&&&&&&@@@@&@@@@@@&&&&@@&&%%&&%%%%%&@&&&&%%%%%%%&&&@&@@&&&&&&&&&&&&%%#((%&&&%(@@@@@@@@@@@@@@@@@@@@@@@@@&&%%%%#(#((((((((((((((((
//##%%%%%%%%%%%%%&&@@@@&&@@@@@@&@&%####################%%&&%%%&&&&&&&&&%%%&&&&&&@@@@@@&&%%%%&@@%&&%&&%#%%%&&&&@&#%%%%&%&&&@&@@&@&@&&&&&&&%%%%#((#&&&%#@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%%(#((((##((((((((((
//&%%%%%%%%%%%%%%&&@@@&@@@@&&@&@@&%####################%%%&&%&&&&&&@@&&&&%%%&&&&&@@&&%%%%%%%&&%%%%%&&&###%&&@&@%#%%%%&%&&&@&@@&@&&&@@&&&&&&%%##(#%&@@%%@@@@@@@@@@@@@@@@@@@@@@@@&&%%%%#(###(((((((((((((
//@&&&%%%%%%%%%%&&&@@@@@@@&&&@&&&%#####################%&#%&#%&&&&&&@@@@@&&%&&&&&&%%%%%%%%%&&%##%%%%%&###%&&@@%#%%%%%&%&&&@&@@&@&&&@@&&&&%%%%#%###%&&&%@@@@@@@@@@@@@@@@@@@@@@@@&&&%%%######((##((((((((
//&&@&&&&%%%%%&&&&&&&&@@&@&&&&&&&#######################&##%#####%%&&@@&@@&&&&&%%%%%%%%%%%%&&####%%%%&###%&&@&#%%%%%&&&&&&@&@@&@&&&&&@@&&&&&%%#%#(%&&@&#@@@@@@@@@@@@@@@@@@@@@@@@&&%%%##(########(((((((
//&&&&&&&&&&&&&&&&&&@@&&&&&&&&&%#######################%%&#%#####%%&@@@@@@&&&&%%%%%%%%%#%#%%##(####%%&####&&&%#%%%%%&&&&&&&&&&&@&@&&&&&&@&@&&&%%%%(&&@@&&@@@@@@@@@@@@@@@@@@@@@@@&&&%%%%(#########((###(
//&&&&&&&&&&&&&&&&@@&&&&&&%%%##########################%%&####(###%%&@@@@@@&&&%%%%%%%%%%%%%%##(#####%%##%#&&%#%%%%%%&&&&&&@&@@&&&&&&&&@&&&&&&%%%%%%#&@@&%@@@@@@@@@@@@@@@@@@@@@@@&&&%%%%%%/((#####((((((
//&&&&&&&&&&&&&&&&&&&%%%%##((((##################&%%##%%#&&##((###%&&&@@@@@&&&&&&&%%%%%%#%#%#((####(%%%%#%%%##%%%%%%&&&@&&&&&&&&&@@@@&@@@&@&&&&%%%%%#@@@&&@@@@@@@@@@@@@@@@@@@@@@@&&%%&&%%###(((((//////
//&&&&&&&&&&&&&&&&&&%%##(((((((#################%&#&&#%%&#&%#((##%%&&@@@@@@&&&&&&%%%%%%%#%%%#((##(#(#%&#######%%%%%%&&&&%&&&&@&&&@@@@@&&@@@@@@&%%%%&&%@@@&@@@@@@@@@@@@@@@@@@@@@@@&&%&%%%%%#/*/////(((((
//@@@@&&&&&&&&&&&&&&&&%#((((((((################%&#%&%#%&&%##(###%%&&@@@@@@&&&&&&&&&%%%%#%%##(###(#((%&#######%%%%%%&&&&%&&&@@&&&@@@@@@&@@@@@@&&%%%&&&&@@&@@@@@@@@@@@@@@@@@@@@@@@&&%%%%%%%%%%#(((((((((
namespace Website.Services
{
    public class MoneyCollectorService
    {
        private StupidLogger _logger;
        private DbContextWrapper _dbContextWrapper;
        private IServiceScopeFactory serviceScopeFactory;
        private StupidBotForSalesBookkeeper _bookkeper;
        private BotsAirstripService _botsAirstripService;

        public MoneyCollectorService(
            StupidLogger _logger,
            IConfiguration configuration,
            StupidBotForSalesBookkeeper _bookkeper,
            BotsAirstripService botsAirstripService,
            IServiceScopeFactory serviceScopeFactory)
        {
            this._logger = _logger;
            this._bookkeper = _bookkeper;
            this._botsAirstripService = botsAirstripService;
            _dbContextWrapper = new DbContextWrapper(configuration);
            this.serviceScopeFactory = serviceScopeFactory;
            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Констуктор сервиса сбора денег.");

            (new Thread(
                () =>
                {
                    CollectPeriodicallyAsync().Wait();
                }
                )).Start();

            //(new thread(collectperiodically))
            //    .Start();

        }

        static object lockObj = new object();

        /// <summary>
        /// При запуске ждёт до 00 05 по UTC и запускает метод снятия денег с аккаунтов
        /// </summary>
        public async Task CollectPeriodicallyAsync()
        {

            

            _logger.Log(LogLevelMyDich.IMPORTANT_INFO, Source.MONEY_COLLECTOR_SERVICE, "Запуск сервиса для сбора денег");

            //Задержка до 5 минут первого часа ночи
            DateTime tomorrow_00_05_00 = DateTime.UtcNow
              .AddDays(1)
              .AddHours(-DateTime.UtcNow.Hour)
              .AddMinutes(-DateTime.UtcNow.Minute + 5)
              .AddSeconds(-DateTime.UtcNow.Second);

            TimeSpan interval = tomorrow_00_05_00 - DateTime.UtcNow;

            _logger.Log(LogLevelMyDich.IMPORTANT_INFO, Source.MONEY_COLLECTOR_SERVICE, $"До первого сбора денег осталось {interval}");
            //Ждёмс
            //Thread.Sleep(interval);

            while (true)
            {
                try
                {
                    await Collect();
                }
                catch (Exception eee)
                {
                    _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Сервис списывания денег упал", ex: eee);
                    Console.WriteLine(eee.Message);
                }

                //Суточная задержка
                //Может просто себя вызвать?
                var day = new TimeSpan(0, 0, 30);
                _logger.Log(LogLevelMyDich.IMPORTANT_INFO, Source.MONEY_COLLECTOR_SERVICE, "Задержка перед следующим запуском списывания денег" + day);
                Thread.Sleep(day);
            }

        }

        //Списание денег со всех аккаунтов
        private async Task Collect()
        {
            int number = 1;
           

            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Старт списывания денег");

            var contextDb = _dbContextWrapper.GetNewDbContext();

            var dt = GetTodayDate().AddDays(-1);
            var blrs = contextDb
                .BotLaunchRecords
                .Where(_blr => _blr.StartTime >= dt)
                .Select(_blr => _blr.BotId)
                .ToList();
            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);
            //Боты, которые запускались сегодня (с повторами)
            List<int> botIds = new List<int>();

            //Убрать дубли
            for (int i = 0; i < blrs.Count; i++)
            {
                if (!botIds.Contains(blrs[i]))
                {
                    botIds.Add(blrs[i]);
                }
            }
            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

            //Боты, которые запускались сегодня (уникальные)
            BotForSalesPrice actualPrice = contextDb
                .BotForSalesPrices
                .Last();

            if (actualPrice == null)
            {
                _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "");
                throw new Exception("Нет тарифа в бд! аааааа");
            }
            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

            //TODO все боты считаются ботами для продаж

            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Количество ботов, которые сегодня запускались = " + botIds.Count);

            //Все боты, которые сегодня работали
            for (int i = 0; i < botIds.Count; i++)
            {
                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                int botId = botIds[i];

                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"В цикле  botId={botId}");

                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                StupidPriceInfo priceInfo = null;
                BotDB bot = null;
                Account account = null;

                try
                {
                priceInfo= _bookkeper.GetPriceInfo(botId);



                    bot = contextDb
                            .Bots
                            .Find(botId);

                    account = contextDb
                            .Accounts
                            .Find(bot.OwnerId);

                }catch(Exception ee)
                {
                    _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Упало согласно ожиданиям");

                }

                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);




                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"priceInfo.SumToday = {priceInfo.SumToday}");
                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"В цикле  account.Id={account.Id}");
                _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);
                //Цена за день адекватная
                if (priceInfo.SumToday > 0)
                {
                    //Транзакции снятия денег с аккаунта за этого бота сегодня уже были?
                    //Может возникнуть, если запущено несколько сервисов списывания денег

                    var date = GetTodayDate();
                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                    WithdrawalLog existingTransaction = contextDb
                        .WithdrawalLog
                        .Where(_wl =>
                            _wl.BotId == botId
                            && _wl.DateTime == date)
                        .SingleOrDefault();

                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                    //если с ботом есть транзакция 
                    if (existingTransaction != null)
                    {
                        _logger.Log(LogLevelMyDich.INFO,
                            Source.MONEY_COLLECTOR_SERVICE,
                            $"existingTransaction != null, existingTransaction.Status={existingTransaction.TransactionStatus}");

                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                        switch (existingTransaction.TransactionStatus)
                        {
                            //Другой сервис снимает деньги
                            case TransactionStatus.TRANSACTION_STARTED:
                                //отложить задачу 
                                break;
                            //Другой сервис снял деньги
                            case TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL:
                                _logger.Log(LogLevelMyDich.ERROR, Source.MONEY_COLLECTOR_SERVICE, "Запущено несколько сервисов списывания денег");
                                //не делать ничего
                                continue;

                            //Другой сервис уронил транзакцию
                            case TransactionStatus.TRANSACTION_FAILED:
                                //произошло дерьмо
                                break;
                            default:
                                //упасть с фатальной ошибкой
                                _logger.Log(
                                    LogLevelMyDich.FATAL,
                                    Source.MONEY_COLLECTOR_SERVICE,
                                    "Неожиданный статус транзакции");
                                throw new Exception("Неожиданный статус транзакции");
                        }

                    }
                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                    var today = GetTodayDate();
                    //Нет начатых транзакций с этим (бот, день)
                    //Записать, что начата транзакция
                    contextDb.WithdrawalLog.Add(new WithdrawalLog()
                    {
                        BotId = botId,
                        AccountId = account.Id,
                        TransactionStatus = TransactionStatus.TRANSACTION_STARTED,
                        TransactionStatusString = TransactionStatus.TRANSACTION_STARTED.ToString(),
                        DateTime = today
                    });
                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                    contextDb.SaveChanges();


                    _logger.Log(LogLevelMyDich.INFO,
                        Source.MONEY_COLLECTOR_SERVICE,
                        $"account.Money = {account.Money}");
                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                    //если у аккаунта есть деньги
                    if (account.Money > 0)
                    {
                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                        if (priceInfo.SumToday >= 0)
                        {

                            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                            //списать деньги
                            account.Money -= priceInfo.SumToday;
                            _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"После списывания на аккаунте осталось {account.Money}", accountId: account.Id);
                        }
                        else
                        {
                            _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Цена бота в день отрицательна!");
                            throw new Exception("Цена бота в день отрицательна!");
                        }
                    }
                    else
                    {

                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, $"На аккаунте нет денег. Остановка всех ботов", accountId: account.Id);


                        //остановить всех ботов, которые принадлежат обанкротившемуся аккаунту
                        List<RouteRecord> rrs = contextDb
                            .RouteRecords
                            .Join(contextDb.Bots,
                                _rr => _rr.BotId,
                                __bot => bot.Id,
                                (_rr, __bot) => new RouteRecord
                                {
                                    BotId = _rr.BotId,
                                    ForestLink = _rr.ForestLink,
                                    Bot = __bot
                                }).ToList();

                        _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                        //По всем ботам этого аккаунта
                        for (int q = 0; q < rrs.Count; q++)
                        {
                            BotDB _bot = rrs[q].Bot;
                            if (_bot != null)
                            {
                                bool bot_belongs_to_the_desired_account = rrs[q].Bot.OwnerId == account.Id;

                                //бот принадлежит обанкротившемуся аккаунту
                                if (bot_belongs_to_the_desired_account)
                                {
                                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE,
                                        $"Вызов остановки бота bot.Id= {_bot.Id}, _bot.OwnerId={_bot.OwnerId} ");

                                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                                    //остановить
                                    JObject result = _botsAirstripService.StopBot(rrs[q].Bot.Id, account.Id);
                                    if (!(bool)result["success"])
                                    {
                                        _logger.Log(
                                            LogLevelMyDich.ERROR,
                                            Source.MONEY_COLLECTOR_SERVICE,
                                            $"Не удалось остановить бота bot.Id= {_bot.Id}, _bot.OwnerId={_bot.OwnerId} failMessage = {result["failMessage"]}");
                                    }

                                }

                            }
                            else
                            {
                                _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE, "Join по RouteRecords не удался");
                            }
                        }
                    }


                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);

                    //Создать запись о удачной транзакции
                    WithdrawalLog withdrawalLog = contextDb
                        .WithdrawalLog
                        .Where(_wl =>
                            _wl.BotId == bot.Id
                            && _wl.DateTime == today)
                        .Single();

                    if (withdrawalLog == null)
                    {
                        _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE,
                            "При подтверждении успешной транзакции не была найдена запись о старте этой транзакции",
                            accountId: account.Id);

                        throw new Exception("При подтверждении успешной транзакции не была найдена запись о старте этой транзакции");

                    }

                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);



                    withdrawalLog.TransactionStatus = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL;
                    withdrawalLog.TransactionStatusString = TransactionStatus.TRANSACTION_COMPLETED_SUCCESSFULL.ToString();
                    withdrawalLog.Price = priceInfo.SumToday;

                    contextDb.SaveChanges();
                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, "Транзакция прошла успешно");

                    _logger.Log(LogLevelMyDich.INFO, Source.MONEY_COLLECTOR_SERVICE, " " + number++);


                }
                else
                {
                    //Произошло дерьмо
                    _logger.Log(LogLevelMyDich.FATAL, Source.MONEY_COLLECTOR_SERVICE,
                        $"Неадекватная цена. botId={botId} цена ={account.Money }");

                    throw new Exception("Неадекватная цена");
                }
            }
        }

        private DateTime GetTodayDate()
        {
            return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
        }
    }
}
