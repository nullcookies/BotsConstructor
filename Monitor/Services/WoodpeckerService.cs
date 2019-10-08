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
        private readonly SimpleLogger _logger;

        public WoodpeckerService(SimpleLogger logger)
        {
            _logger = logger;
        }
        
        private List<string> _targetUrls = new List<string>
        {
            //forest
            "http://localhost:8080/MonitorNegotiator/Ping",
            //website
            "https://localhost:5001/MonitorNegotiator/Ping"
        };


        public async void StartPingAsync(int delaySec = 1, List<string> targetUrls= null)
        {
            _logger.Log(LogLevel.INFO,Source.MONITOR,"Старт сервиса для пингования монитором");

            _isWorking = true;
            
            if (targetUrls != null && targetUrls.Count>0) _targetUrls = targetUrls;

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
                            LogLevel.INFO,
                            Source.MONITOR,
                            $"Успешный пинг по url={url}");
                    }
                    catch (Exception exception)
                    {
                        _logger.Log(
                            LogLevel.ERROR,
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