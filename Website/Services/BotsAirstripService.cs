using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Website.Other;
using Website.Other.Filters;
using Website.Services;
using Website.Services.Bookkeeper;
namespace Website.Services
{
    //Как сюда пробросить локализацию?
    //Или это нужно делать не здесь?
    public class BotsAirstripService
    {
        StupidLogger _logger;
        ApplicationContext _contextDb;

        public BotsAirstripService(StupidLogger logger, ApplicationContext contextDb)
        {
            _logger = logger;
            _contextDb = contextDb;
        }

        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <param name="botId">Id бота в БД</param>
        /// <param name="accountId">Id аккаунта, для которого выполняется запуск. Например: создатель бота или администратор бота.</param>
        /// <returns> JObject со статусом запроса.</returns>
        public JObject StartBot(int botId, int accountId)
        {
            BotDB bot = _contextDb.Bots.Find(botId);
            JObject jObject = null;

            //Аккаунт, по запросу которого бот запускается
            Account account = _contextDb.Accounts.Find(accountId);

            //TODO вынести в другой сервис
            bool account_can_run_a_bot = bot.OwnerId == accountId;

            //Аккаунт может запускать бота?
            if (!account_can_run_a_bot)
            {
                _logger.Log(LogLevelMyDich.WARNING,
                      Source.BOTS_AIRSTRIP_SERVICE,
                      "Попытка запуска бота аккаунтом, который не имеет к нему доступа.",
                      accountId: accountId);

                jObject = new JObject()
                            {
                                { "success", false},
                                {"failMessage", " У вас нет доступа к этому боту." }
                            };
                return jObject;

            }

            Account botOwner = _contextDb.Accounts.Find(bot.OwnerId);

            //У собственника бота есть деньги?
            if (botOwner.Money < 0)
            {
                _logger.Log(LogLevelMyDich.WARNING,
                     Source.BOTS_AIRSTRIP_SERVICE,
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
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить бота без токена. botId={botId}");
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
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить бота без разметки. botId={botId}");
                jObject = new JObject()
                {
                    { "success", false},
                    {"failMessage", "Запуск бота не возможен без разметки. Нажмите \"Редактировать разметку черновика\"" }
                };
                return jObject;
            }

            //Если бот уже запущен, вернуть ошибку
            RouteRecord existingRecord = _contextDb.RouteRecords.Find(botId);
            if (existingRecord != null)
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Попытка запутить запущенного бота.");
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
                var result = Stub.SendPost(forestUrl, data).Result;

                JObject answer = (JObject)JsonConvert.DeserializeObject(result);

                bool successfulStart = (bool)answer["success"];
                string failMessage = (string)answer["failMessage"];

                if (successfulStart)
                {
                    //Лес нормально сделал запись о запуске?
                    RouteRecord rr = _contextDb.RouteRecords.Find(botId);
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
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR,
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

                _logger.Log(LogLevelMyDich.ERROR, Source.WEBSITE, $"Не удалось запустить бота. botId={botId}. ex.Message={ex.Message}");

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

            JObject jObject = null;
            BotDB bot = _contextDb.Bots.Find(botId);

            if (bot != null)
            {
                bool account_can_stop_the_bot = false;

                //TODO вынести в другой сервис
                if (bot.OwnerId == accountId)
                {
                    account_can_stop_the_bot = true;
                }

                if (account_can_stop_the_bot)
                {
                    RouteRecord record = _contextDb.RouteRecords.Find(bot.Id);

                    if (record != null)
                    {
                        try
                        {
                            //запрос на остановку бота

                            string forestUrl = record.ForestLink + "/Home/StopBot";
                            string data = "botId=" + bot.Id;
                            var result = Stub.SendPost(forestUrl, data).Result;

                            RouteRecord rr = _contextDb
                                .RouteRecords
                                .Where(_rr => _rr.BotId == botId)
                                .SingleOrDefault();

                            if (rr == null)
                            {
                                _logger.Log(LogLevelMyDich.INFO, Source.BOTS_AIRSTRIP_SERVICE, "Лес нормально удалил запись о боте");
                                jObject = new JObject()
                                {
                                    { "success", true}
                                };
                                return jObject;
                            }
                            else
                            {
                                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота botId={botId}," +
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
                            _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
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
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Source.WEBSITE, $"При остановке бота(botId={bot.Id}, " +
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
                    _logger.Log(LogLevelMyDich.WARNING,
                        Source.BOTS_AIRSTRIP_SERVICE,
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
