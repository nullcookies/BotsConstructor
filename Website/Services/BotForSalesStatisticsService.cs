using DataLayer;
using DataLayer.Models;
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

namespace Website.Services
{
    public class BotForSalesStatisticsService
    {

        public BotForSalesStatisticsService(StupidLogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextWrapper = new DbContextFactory();

            PeriodicFooAsync(TimeSpan.FromSeconds(10), CancellationToken.None);
        }


        //Периодический запуск
        private async Task<bool> PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                await SendRelevantStatistics();
                await Task.Delay(interval, cancellationToken);
            }
        }


        private async Task SendRelevantStatistics()
        {
         
            //выбрать всю статистику для ботов

            List<BotForSalesStatistics> allStat = _contextDb.BotForSalesStatistics.ToList();
            List<RouteRecord> rrs = _contextDb.RouteRecords.ToList();
            List<int> workingBotIds= new List<int>();

            foreach (var routeRecord in rrs)
            {
                workingBotIds.Add(routeRecord.BotId);
            }

        
            try
            {

                //Перебор статистики по всем ботам
                for (int i = 0; i < allStat.Count; i++)
                {
                    int botId = allStat[i].BotId;

                    //Если есть подписанные на этого бота
                    if (_dict_botId_Websockets.ContainsKey(botId))
                    {
                        BotForSalesStatistics_Websockets bfs_websockets = _dict_botId_Websockets[botId];

                        //Какое-то значение поменялось
                        if (bfs_websockets.BotForSalesStatisticsOld.NumberOfOrders != allStat[i].NumberOfOrders ||
                            bfs_websockets.BotForSalesStatisticsOld.NumberOfUniqueUsers != allStat[i].NumberOfUniqueUsers ||
                            bfs_websockets.BotForSalesStatisticsOld.NumberOfUniqueMessages != allStat[i].NumberOfUniqueMessages ||
                            bfs_websockets.IsWorking != workingBotIds.Contains(botId))
                        {
                            for (int j = 0; j < bfs_websockets.WebSockets.Count; j++)
                            {
                                //Отправка нового значения

                                WebSocket webSocket = bfs_websockets.WebSockets[j];
                                BotForSalesStatistics stat = allStat[i];

                                JObject JObj = new JObject
                                {
                                    { "botWorks",      workingBotIds.Contains(botId)},
                                    { "ordersCount",    stat.NumberOfOrders},
                                    { "usersCount",     stat.NumberOfUniqueUsers},
                                    { "messagesCount",  stat.NumberOfUniqueMessages},
                                };

                                string jsonString = JsonConvert.SerializeObject(JObj);
                                var bytes = Encoding.UTF8.GetBytes(jsonString);
                                var arraySegment = new ArraySegment<byte>(bytes);

                                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                            }

                            //Обновление значений до текущих
                            bfs_websockets.BotForSalesStatisticsOld.NumberOfOrders = allStat[i].NumberOfOrders;
                            bfs_websockets.BotForSalesStatisticsOld.NumberOfUniqueUsers = allStat[i].NumberOfUniqueUsers;
                            bfs_websockets.BotForSalesStatisticsOld.NumberOfUniqueMessages = allStat[i].NumberOfUniqueMessages;
                            bfs_websockets.IsWorking = workingBotIds.Contains(botId);

                        }

                        bfs_websockets.BotForSalesStatisticsOld = allStat[i];

                    }
                }



            }
            catch (Exception ee)
            {
                _logger.Log(LogLevelMyDich.ERROR, Source.WEBSITE, "Сайт. При отправке статистики бота по websocket произошла ошибка. " + ee.Message);
            }
        }

     
        public void RegisterInNotificationSystem(int botId, WebSocket webSocket)
        {

            if (_dict_botId_Websockets.ContainsKey(botId))
            {

                _dict_botId_Websockets[botId].WebSockets.Add(webSocket);

                //Сброс значений
                _dict_botId_Websockets[botId].BotForSalesStatisticsOld = new BotForSalesStatistics();
                //Для того, чтобы значения обновились на новых вкладках 
            }
            else
            {
                _dict_botId_Websockets.TryAdd(
                    botId, 
                    new BotForSalesStatistics_Websockets()
                    {
                        WebSockets = new List<WebSocket>
                        {
                            webSocket
                        }
                    }
                );
            }
            
        }


        readonly DbContextFactory _dbContextWrapper;
        readonly StupidLogger _logger;
        ApplicationContext _contextDb
        {
            get
            {
                return _dbContextWrapper.GetNewDbContext();
            }
        }

        ConcurrentDictionary<int, BotForSalesStatistics_Websockets> _dict_botId_Websockets = new ConcurrentDictionary<int, BotForSalesStatistics_Websockets>();
        
       private class BotForSalesStatistics_Websockets
        {
            public List <WebSocket> WebSockets = new List<WebSocket>();
            public BotForSalesStatistics BotForSalesStatisticsOld = new BotForSalesStatistics();
            public bool IsWorking =false;
        }
    }
}
