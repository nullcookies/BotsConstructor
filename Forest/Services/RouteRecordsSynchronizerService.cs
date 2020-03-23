using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using MyLibrary;

namespace Forest.Services
{
    public class RouteRecordsSynchronizerService
    {
        private bool _isWorking;
        private readonly SimpleLogger _logger;
        
        public RouteRecordsSynchronizerService(SimpleLogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _isWorking = true;
            (new System.Threading.Thread(SyncPeriodic)).Start();
        }

        public void Stop()
        {
            _isWorking = false;
        }

        private async void SyncPeriodic()
        {
            while (true)
            {
                if(!_isWorking)
                    break;;
                
                CheckBots();

                await Task.Delay(1000 * 1);
            }
        }

        private void CheckBots()
        {
            
            _logger.Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                $"Старт синхронизации ботов");
            
            var contextDb = new DbContextFactory().CreateDbContext();
            var routeRecords = contextDb.RouteRecords.Include(rr=>rr.Bot).ToArray();
            
            //TODO только один лес

            List<string> botUsernamesFromTable = new List<string>();
            foreach (var routeRecord in routeRecords)
            {
                botUsernamesFromTable.Add(routeRecord.Bot.BotName);
            }
            
            string loggerComment = string.Join(",", botUsernamesFromTable);
            _logger.Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                $" Список ботов, которые должны работать в лесу = {loggerComment}");
            
            foreach (var myBotName in BotsStorage.BotsDictionary.Keys)
            {
                
                _logger.Log(
                    LogLevel.IMPORTANT_INFO,
                    Source.FOREST,
                    $"Проверка botName={myBotName}.");
                
                
                if (!botUsernamesFromTable.Contains(myBotName))
                {
                    _logger.Log(
                        LogLevel.IMPORTANT_INFO,
                        Source.FOREST,
                        "Лес обнаружил, что у него работает бот, которого быть не должно" +
                        $"BotName={myBotName}. Старт убивания этого бота");
                    
                    
                    //Убрать из списка
                    botUsernamesFromTable.Remove(myBotName);
                    
                    //В этом лесу работает бот, которого тут быть не должно
                    //Убить этого бота    
                    
                    //Остановка
                    BotsStorage.BotsDictionary[myBotName].Stop();
                    //Убивание
                    BotsStorage.BotsDictionary.Remove(myBotName);
                    
                    _logger.Log(
                        LogLevel.IMPORTANT_INFO,
                        Source.MONITOR,
                        $"Бот с именем {myBotName} убит в лесу");
                    
                }
                else
                {
                    _logger.Log(
                        LogLevel.IMPORTANT_INFO,
                        Source.FOREST,
                        $"Проверка botName={myBotName} прошла успешно");
                }
            }

            if (botUsernamesFromTable.Count != 0)
            {
                //Из записей следует, что в этом лесу нет некоторых ботов
                //Ну и хрен с ними
                //Пусть с этим разбирается монитор
            }
            

            
            _logger.Log(
                LogLevel.IMPORTANT_INFO,
                Source.FOREST,
                $"Финиш синхронизации ботов");
        }
    }
}