using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer;
using MyLibrary;

namespace Monitor.Services
{
    public class WoodpeckerService
    {
        private bool _isWorking;
        private readonly StupidLogger _logger;

        public WoodpeckerService(StupidLogger logger)
        {
            _logger = logger;
            _logger.Log(LogLevelMyDich.INFO,Source.MONITOR,"Старт сервиса для пингования монитором");
        }
        
        private List<string> _targetUrls = new List<string>()
        {
            //forest
            "http://localhost:8080/Monitor/Ping",
            //website
            "https://localhost:5001/Monitor/Ping"
        };


        public async void StartPingAsync(int delaySec = 1, List<string> targetUrls= null)
        {
            _isWorking = true;
            
            if (targetUrls != null && targetUrls.Count>0)
            {
                _targetUrls = targetUrls;
            }
            
            while (true)
            {
                if(!_isWorking)
                    break;

                foreach (var url in _targetUrls)
                {
                    try
                    {
                        Ping(url);
                        _logger.Log(
                            LogLevelMyDich.INFO,
                            Source.MONITOR,
                            $"Успешный пинг по url={url}");
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(
                            LogLevelMyDich.ERROR,
                            Source.MONITOR,
                            $"Ошибка в сервисе пинга в мониторе. Url={url}", 
                            ex:exception);
                    }
                }

                await Task.Delay(1000 * delaySec);

            }
        }

        public void StopPing()
        {
            _isWorking = false;
        }
        
        private void Ping(string url)
        {
            Stub.SendPostAsync(url).Wait();
        }
    }
}