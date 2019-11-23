using DataLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MyLibrary;
using Website.Other;
using Website.Other.Filters;
using Website.Services;


namespace Website.Services
{
  
    public class BotsAirstripService
    {
        readonly SimpleLogger _logger;
        private readonly DbContextFactory _dbContextWrapper;

        public BotsAirstripService(SimpleLogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextWrapper = new DbContextFactory();
        }

        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <param name="botId">Id бота в БД</param>
        /// <param name="accountId">Id аккаунта, для которого выполняется запуск. Например: создатель бота или администратор бота.</param>
        /// <returns> JObject со статусом запроса.</returns>
        public JObject StartBot(int botId, int accountId)
        {
            ApplicationContext contextDb = _dbContextWrapper.GetNewDbContext();
            BotDB bot = contextDb.Bots.Find(botId);
            JObject jObject = null;

            //Аккаунт, по запросу которого бот запускается
            Account account = contextDb.Accounts.Find(accountId);

            //TODO вынести в другой сервис
            bool accountCanRunABot = bot.OwnerId == accountId;

            //Аккаунт может запускать бота?
            if (!accountCanRunABot)
            {
                _logger.Log(LogLevel.WARNING,
                      Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                      "Попытка запуска бота аккаунтом, который не имеет к нему доступа.",
                      accountId: accountId);

                jObject = new JObject()
                    {
                        { "success", false},
                        {"failMessage", " У вас нет доступа к этому боту." }
                    };
                return jObject;

            }

            Account botOwner = contextDb.Accounts.Find(bot.OwnerId);

            //У собственника бота есть деньги?
            if (botOwner.Money < 0)
            {
                _logger.Log(LogLevel.WARNING,
                     Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                     "Попытка запуска бота с маленьким количеством средств на счету.",
                     accountId: accountId);

                jObject = new JObject()
                    {
                        { "success", false},
                        {"failMessage", " Недостаточно средств на счету собственника бота." }
                    };
                return jObject;
            }

            //без токена запускаться нельзя
            if (bot.Token == null)
            {
                _logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, 
                    $"Попытка запутить бота без токена. botId={botId}");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Запуск бота не возможен без токена. Установите токен в настройках бота." }
                };
                return jObject;
            }

            //без разметки запускаться нельзя
            if (bot.Markup == null)
            {
                _logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE,
                    $"Попытка запутить бота без разметки. botId={botId}");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Запуск бота не возможен без разметки. Нажмите \"Редактировать разметку черновика\"" }
                };
                return jObject;
            }

            //Если бот уже запущен, вернуть ошибку
            RouteRecord existingRecord = contextDb.RouteRecords.Find(botId);
            if (existingRecord != null)
            {
                _logger.Log(LogLevel.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, 
                    $"Попытка запутить запущенного бота.");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Этот бот уже запущен. (" }
                };
                return jObject;
            }
                       
            //Попытка запуска в лесу
            try
            {
                //TODO брать ссылку из монитора
                string forestUrl = "http://localhost:8080/Home/RunNewBot";

                string data = "botId=" + botId;
                var result = Stub.SendPostAsync(forestUrl, data).Result;

                JObject answer = (JObject)JsonConvert.DeserializeObject(result);

                bool successfulStart = (bool)answer["success"];
                string failMessage = (string)answer["failMessage"];

                if (successfulStart)
                {
                    //Лес нормально сделал запись о запуске?
                    RouteRecord rr = contextDb.RouteRecords.Find(botId);
                    if (rr != null)
                    {
                        jObject = new JObject()
                        {
                            { "success", true}
                        };
                        return jObject;
                    }
                    else
                    {
                        _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR,
                            Source.WEBSITE, 
                            $"Лес вернул Ок (нормальный запуск бота), но не сделал запись в бд. botId={botId}");

                        jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Ошибка сервера при запуске бота" }
                        };
                        return jObject;

                    }
                }
                else
                {
                    jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Ошибка сервера при запуске бота."+failMessage }
                        };
                    return jObject;
                }

            }
            catch (Exception ex)
            {

                _logger.Log(LogLevel.ERROR, Source.WEBSITE, $"Не удалось запустить бота. botId={botId}. ex.Message={ex.Message}");

                jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", "Не удалось запустить бота. Возможно, возникла проблема соединения" }
                        };
                return jObject;
            }


        }

        /// <summary>
        /// Остановка бота
        /// </summary>
        /// <param name="botId">Id бота в БД</param>
        /// <param name="accountId">Id аккаунта, для которого выполняетсся остановка. Например: создатель бота или администратор бота.</param>
        /// <returns> JObject со статусом запроса.</returns>
        public JObject StopBot(int botId, int accountId)
        {

            ApplicationContext contextDb = _dbContextWrapper.GetNewDbContext();

            JObject jObject = null;
            BotDB bot = contextDb.Bots.Find(botId);

            if (bot != null)
            {
                bool accountCanStopTheBot = bot.OwnerId == accountId;

                //TODO вынести в другой сервис

                if (accountCanStopTheBot)
                {
                    RouteRecord record = contextDb.RouteRecords.Find(bot.Id);

                    if (record != null)
                    {
                        try
                        {
                            //запрос на остановку бота

                            string forestUrl = record.ForestLink + "/Home/StopBot";
                            string data = "botId=" + bot.Id;
                            var result = Stub.SendPostAsync(forestUrl, data).Result;

                            RouteRecord rr = contextDb
                                .RouteRecords
                                .SingleOrDefault(_rr => _rr.BotId == botId);

                            if (rr == null)
                            {
                                _logger.Log(LogLevel.INFO, Source.WEBSITE_BOTS_AIRSTRIP_SERVICE, $"Бот {bot.Id} был нормально остановлен.");
                                jObject = new JObject()
                                {
                                    { "success", true}
                                };
                                return jObject;
                            }
                            else
                            {
                                _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота botId={botId}," +
                                    $" accountId={accountId}. Лес ответил Ok, но не удалил RouteRecord из БД ");

                                jObject = new JObject()
                                    {
                                        { "success", false},
                                        {"failMessage", " Не удалось остановить бота." }
                                    };
                                return jObject;
                            }

                        }
                        catch (Exception exe)
                        {
                            _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) не удалось выполнить post запрос на лес." +
                            $"Exception message ={exe.Message}");

                            jObject = new JObject()
                                    {
                                        { "success", false},
                                        {"failMessage", " Не удалось остановить бота. Возможно, есть проблемы с соединением." }
                                    };
                            return jObject;
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
                            $"ownerId={bot.OwnerId}, пользователем accountId={accountId}) в БД не была найдена" +
                            $"запись о сервере на котором бот работает. Возможно, она была удалена или не добавлена.");

                        jObject = new JObject()
                            {
                                { "success", false},
                                {"failMessage", " Бот уже остановлен." }
                            };
                        return jObject;

                    }

                }
                else
                {
                    _logger.Log(LogLevel.WARNING,
                        Source.WEBSITE_BOTS_AIRSTRIP_SERVICE,
                        "Попытка остановки бота аккаунтом, который не имеет к нему доступа.",
                        accountId:accountId);

                    jObject = new JObject()
                            {
                                { "success", false},
                                {"failMessage", " У вас нет доступа к этому боту." }
                            };
                    return jObject;

                }

            }
            else
            {
                jObject = new JObject()
                        {
                            { "success", false},
                            {"failMessage", " Такого бота не существует." }
                        };
                return jObject;
            }

        }
    }
}
