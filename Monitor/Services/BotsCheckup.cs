using System;
using System.Collections.Generic;
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
            _logger.Log(LogLevel.INFO,Source.MONITOR,"Старт сервиса для проверки работы ботов");
        }
        
//        private List<string> _targetUrls = new List<string>()
//        {
//            //forest
//            "http://localhost:8080/Monitor/BotIsHere"
//        };


        public async void StartCheckupAsync(int delaySec = 1, List<string> targetUrls= null)
        {
            _isWorking = true;
            
//            if (targetUrls != null && targetUrls.Count>0)
//            {
//                _targetUrls = targetUrls;
//            }
            
            while (true)
            {
                if(!_isWorking)
                    break;

                var contextDb = new DbContextFactory().GetNewDbContext();
                
                var routeRecords = contextDb.RouteRecords;
                
                foreach (var routeRecord in routeRecords)
                {
                    string link = routeRecord.ForestLink + "/Monitor/BotIsHere";
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

                await Task.Delay(1000 * delaySec);

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