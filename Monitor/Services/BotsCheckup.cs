using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Monitor.Services
{
    public class BotsCheckup
    {
        private bool _isWorking;
        private readonly StupidLogger _logger;

        public BotsCheckup(StupidLogger logger)
        {
            _logger = logger;
        }


        public async void StartCheckupAsync(int delaySec = 1, List<string> targetUrls= null)
        {
            _logger.Log(LogLevel.INFO,Source.MONITOR,"Старт сервиса для проверки работы ботов");

            _isWorking = true;
            
            while (true)
            {
                if(!_isWorking)
                    break;

                await Task.Delay(1000 * delaySec);
                
                var contextDb = new DbContextFactory().GetNewDbContext();
                
                var routeRecords = contextDb.RouteRecords.ToArray();

                if (routeRecords.Length == 0)
                {
                    _logger.Log(LogLevel.INFO,Source.MONITOR,"При проверке работоспособности ботов в лесах монитором в таблице ботов не было найдено ни одного");
                    continue;
                }
                
                foreach (var routeRecord in routeRecords)
                {
                    string link = routeRecord.ForestLink + "/MonitorNegotiator/BotIsHere";
                    try
                    {
                        await Checkup(link, routeRecord.BotId);
                        
                        _logger.Log(
                            LogLevel.INFO,
                            Source.MONITOR,
                            $"Успешная проверка наличия бота в лесу. " +
                            $"botId={routeRecord.BotId} url={link}");
                        
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(
                            LogLevel.ERROR,
                            Source.MONITOR,
                            $"Ошибка проверки наличия бота в лесу." +
                            $"botId={routeRecord.BotId} url={link}", 
                            ex:exception);
                        
                        //удаление неверной routeRecord
                        contextDb.RouteRecords.Remove(routeRecord);
                        contextDb.SaveChanges();
                    }
                }


            }
        }

        private async Task Checkup(string forestUrl, int botId)
        {
            string jsonAnswer = await Stub.SendPostAsync(forestUrl, $"botId={botId}");
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonAnswer);

            if (!(bool) obj["success"])
            {
                string failMessage = (string) obj["failMessage"];
                throw new Exception("Лес вернул отрицательный ответ (у него нет бота с таким id)" +
                                    $"failMessage = {failMessage} ");
            }
        }

        public void StopCheckup()
        {
            _isWorking = false;
        }
        
      
    }
}