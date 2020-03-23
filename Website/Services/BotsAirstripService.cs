using DataLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using MyLibrary;

namespace Website.Services
{
    public class BotsAirstripService:IBotStorageNegotiator
    {
        private readonly ILogger logger;
        private readonly AccessCheckService accessCheckService;
        private readonly ApplicationContext dbContext;
        
        private readonly IForestNegotiatorService forestNegotiatorService;

        public BotsAirstripService(ILogger logger, 
            AccessCheckService accessCheckService, MonitorNegotiatorService monitorNegotiatorService,
            IForestNegotiatorService forestNegotiatorService,  ApplicationContext dbContext)
        {
            this.logger = logger;
            this.accessCheckService = accessCheckService;
            this.forestNegotiatorService = forestNegotiatorService;
            this.dbContext = dbContext;
        }
        
        public BotStartMessage StartBot(int botId, int accountId)
        {
            BotStartMessage result;
            BotDB bot = dbContext.Bots.Find(botId);
            
            //Такой бот существует?
            if (bot == null)
            {
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.BotWithSuchIdDoesNotExist
                };
                return result;
            }

            //Аккаунт может запускать бота?
            bool accountCanRunABot = accessCheckService.IsAccountCanRunBot(bot, accountId);
            if (!accountCanRunABot)
            {
                logger.Log(LogLevel.WARNING,
                      Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                      "Попытка запуска бота аккаунтом, который не имеет к нему доступа.",
                      accountId);
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.NoAccessToThisBot
                };
                return result;
            }
            
            //У собственника бота есть деньги?
            Account botOwner = dbContext.Accounts.Find(bot.OwnerId);
            if (botOwner.Money <= 0)
            {
                logger.Log(LogLevel.WARNING,
                     Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                     "Попытка запуска бота с маленьким количеством средств на счету.",
                     accountId: accountId);

                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.NotEnoughFundsInTheAccountOfTheBotOwner
                };
                
                return result;
            }

            //без токена запускаться нельзя
            if (bot.Token == null)
            {
                logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, 
                    $"Попытка запутить бота без токена. botId={botId}");
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.TokenMissing
                };
                return result;
            }

            //без разметки запускаться нельзя
            if (bot.Markup == null)
            {
                logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE,
                    $"Попытка запутить бота без разметки. botId={botId}");
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.NoMarkupData
                };
                return result;
            }

            //Если бот уже запущен, вернуть ошибку
            RouteRecord existingRecord = dbContext.RouteRecords.Find(botId);
            if (existingRecord != null)
            {
                logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, 
                    $"Попытка запутить запущенного бота.");
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.ThisBotIsAlreadyRunning
                };
                return result;
            }
                       
            //Попытка запуска
            try
            {
                string forestAnswer = forestNegotiatorService.SendStartBotMessage(botId);

                JObject answer = (JObject)JsonConvert.DeserializeObject(forestAnswer);
                
                bool successfulStart = (bool)answer["success"];
                string failMessage = (string)answer["failMessage"];

                //Лес вернул ок?
                if (!successfulStart)
                {
                    result = new BotStartMessage
                    {
                        Success = false,
                        FailureReason = BotStartFailureReason.ServerErrorWhileStartingTheBot,
                        ForestException = failMessage
                    };
                    return result;
                }

                //Лес сохранил данные для про запуск в БД?
                RouteRecord rr = dbContext.RouteRecords.SingleOrDefault(record => record.BotId == botId);
                if (rr == null)
                {
                    return new BotStartMessage
                    {
                        Success = false,
                        FailureReason = BotStartFailureReason.ServerErrorWhileStartingTheBot
                    };
                }
                
                //Ну тоды всё хорошо.
                return new BotStartMessage{Success = true};
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.ERROR, Source.WEBSITE, $"Не удалось запустить бота. botId={botId}. " +
                                                           $"ex.Message={ex.Message}");
                result = new BotStartMessage
                {
                    Success = false,
                    FailureReason = BotStartFailureReason.ConnectionError
                };
                return result;
            }
        }
        
        public BotStopMessage StopBot(int botId, int accountId)
        {
            //Такой бот существует?
            BotDB bot = dbContext.Bots.Find(botId);
            if (bot == null)
            {
                return new BotStopMessage
                {
                    Success = false,
                    FailureReason = BotStopFailureReason.BotWithSuchIdDoesNotExist
                };
            }
            
            //Аккаунт может останавливать бота?
            bool accountCanStopTheBot = accessCheckService.IsAccountCanStopBot(bot, accountId);
            if (!accountCanStopTheBot)
            {
                
                logger.Log(LogLevel.WARNING,
                    Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                    "Попытка остановки бота аккаунтом, который не имеет к нему доступа.",
                    accountId);
                return new BotStopMessage
                {
                    Success = false,
                    FailureReason = BotStopFailureReason.NoAccessToThisBot
                };
            }
            
            //Бот запущен?
            RouteRecord record = dbContext.RouteRecords.Find(bot.Id);
            if (record == null)
            {
                logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE, 
                    $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) в БД не была найдена" +
                            $"запись о сервере на котором бот работает. Возможно, она была удалена или не добавлена.");

                return new BotStopMessage
                {
                    Success = false,
                    FailureReason = BotStopFailureReason.ThisBotIsAlreadyStopped
                };
            }
            
            try
            {
                //запрос на остановку бота
                var forestAnswer = forestNegotiatorService.SendStopBotMessage(bot.Id);

                JObject answer = (JObject) JsonConvert.DeserializeObject(forestAnswer);
                bool successfulStart = (bool) answer["success"];
                string failMessage = (string) answer["failMessage"];

                //Лес вернул ок?
                if (!successfulStart)
                {
                    return new BotStopMessage
                    {
                        Success = false,
                        FailureReason = BotStopFailureReason.ServerErrorWhileStoppingTheBot,
                        ForestException = failMessage
                    };
                }

                //Лес удалил данные про бота?
                RouteRecord rr = dbContext
                    .RouteRecords
                    .SingleOrDefault(_rr => _rr.BotId == botId);

                if (rr == null)
                {
                    logger.Log(LogLevel.INFO, Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                        $"Бот {bot.Id} был нормально остановлен.");
                    return new BotStopMessage {Success = true};
                }
                else
                {
                    logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE,
                        $"При остановке бота botId={botId}," +
                        $" accountId={accountId}. Лес ответил Ok, но не удалил RouteRecord из БД ");
                    return new BotStopMessage
                    {
                        Success = false,
                        FailureReason = BotStopFailureReason.ServerErrorWhileStoppingTheBot
                    };
                }

            }
            catch (Exception exe)
            {
                logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE,
                    $"При остановке бота(botId={bot.Id}, " +
                    $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) не удалось выполнить post запрос на лес." +
                    $"Exception message ={exe.Message}");
                return new BotStopMessage
                {
                    Success = false,
                    FailureReason = BotStopFailureReason.ConnectionError
                };
            }
        }
    }
}
