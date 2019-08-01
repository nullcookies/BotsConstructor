using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using Website.Models;
using Website.Other;

namespace Website.Services
{
    public class OrdersCountNotificationService
    {
        ApplicationContext _contextDb;

        public OrdersCountNotificationService(IConfiguration configuration)
        {
            
            #region Дублирование кода
            string connectionString;
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
                connectionString = configuration.GetConnectionString("PostgresConnectionDevelopment");
            else
                connectionString = configuration.GetConnectionString("PostgresConnectionLinux");

            _contextDb = new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql(connectionString)
                .Options
            );

            #endregion

            PeriodicFooAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

        }



        public async Task PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                
                Console.WriteLine("Общая функция перед выполнением задачи");
                await SendNotificationsOfNewOrders();
                Console.WriteLine("Общая функция после выполнением задачи перед задержкой");
                await Task.Delay(interval, cancellationToken);
                Console.WriteLine("Общая функция после задержки");
            }
        }
    
        private async Task SendNotificationsOfNewOrders()
        {
            Console.WriteLine("Отправка уведомлений");

            List<Order> allOrders = null;
            //Лок вообще помогает?
            lock (lockObj)
            {
                allOrders = _contextDb.Orders.ToList();
            }


            //Перебираю все заказы
            for (int i = 0; i < allOrders.Count; i++)
            {
                Console.WriteLine($"Перебор заказов i={i}");
                Order order = allOrders[i];

                //Это новый заказ
                if (order.OrderStatus != null)
                {
                    int botId = order.BotId;

                    //У нас есть аккаунты, которые подписаны на этого бота
                    if (_dict_botId_accountIds.ContainsKey(botId))
                    {
                        List<int> accountIds = _dict_botId_accountIds[botId];
                        //Инкремент счётчика аккаунта
                        for (int j = 0; j < accountIds.Count; j++)
                        {
                            int accountId = accountIds[j];
                            _dict_accountId_WebSocket[accountId].OrdersCount++;
                        }
                    }
                }
            }

            Console.WriteLine("Отправка новых значений");
            Console.WriteLine($"_dict_accountId_WebSocket.Count = {_dict_accountId_WebSocket.Count}");

            try
            {

                //Отправка новых чисел в счётчике
            foreach (var key in _dict_accountId_WebSocket.Keys)
            {

                int ordersCount = _dict_accountId_WebSocket[key].OrdersCount;

                Console.WriteLine($"ordersCount={ordersCount}");

                WebSocket webSocket = _dict_accountId_WebSocket[key].WebSocket;

                JObject JObj = new JObject
                    {
                        { "ordersCount", 554}
                    };

                string jsonString = JsonConvert.SerializeObject(JObj);
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                var arraySegment = new ArraySegment<byte>(bytes);
                Console.WriteLine(webSocket.State);
                Console.WriteLine(webSocket.SubProtocol);
                
                
                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

                _dict_accountId_WebSocket[key].OrdersCount = 0;
            }

            }catch(Exception ee)
            {
                Console.WriteLine(ee.Message);
            }


        }



        /// <summary>
        /// Словарь для того, чтобы знать куда отправлять уведомление
        /// </summary>
        private static ConcurrentDictionary<int, OrdersCountModel> _dict_accountId_WebSocket = new ConcurrentDictionary<int, OrdersCountModel>();

        /// <summary>
        /// Словарь для того, чтобы знать кому отправлять уведомления
        /// </summary>
        private static ConcurrentDictionary<int, List<int>> _dict_botId_accountIds = new ConcurrentDictionary<int, List<int>>();


        

        public async Task RegisterInNotificationSystem(int accountId, WebSocket webSocket)
        {
            Console.WriteLine(  $"Регистрация аккаунта {accountId}");
            _dict_accountId_WebSocket.TryAdd(accountId, new OrdersCountModel(webSocket) );
            lock (lockObj)
            {


                //На каких ботов подписать аккаунт?

                //Боты, которые принадлежат аккаунту
                List<int> the_bot_ids_on_which_the_account_is_signed = 
                    _contextDb.Bots.Where(_bot => _bot.OwnerId == accountId).Select(_bot=>_bot.Id).ToList();
                
                //TODO Боты, к которым аккаунт имеет доступ(модератор)

                for (int i = 0; i < the_bot_ids_on_which_the_account_is_signed.Count; i++)
                {
                    int botId = the_bot_ids_on_which_the_account_is_signed[i];
                    Console.WriteLine("Бот на который аккаунт подписался botId" + botId);

                    List<int> accounts_that_are_subscribed_to_bot = null;

                    _dict_botId_accountIds.TryGetValue(botId, out accounts_that_are_subscribed_to_bot);

                    if (accounts_that_are_subscribed_to_bot != null 
                        && accounts_that_are_subscribed_to_bot.Count > 0)
                    {
                        Console.WriteLine($"аккаунт {accountId} подписался на получение заказов от бота {botId}");
                        accounts_that_are_subscribed_to_bot.Add(accountId);
                    }
                    else
                    {
                        _dict_botId_accountIds.TryAdd(botId, new List<int>() { accountId });
                        Console.WriteLine(  "Не было подписано ни одно аккаунта на этого бота");
                    }
                }
            }

            
            int counter = 0;
            while (true)
            {

                JObject JObj = new JObject
                    {
                        { "ordersCount", counter++}
                    };

                string jsonString = JsonConvert.SerializeObject(JObj);
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                var arraySegment = new ArraySegment<byte>(bytes);
                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

                //Не обрыв
                Thread.Sleep(1000);

                //Обрыв
                //await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            }



        }


        object lockObj = new object();

        private class OrdersCountModel
        {
            public int OrdersCount;
            public readonly WebSocket WebSocket;

            public OrdersCountModel(WebSocket webSocket)
            {
                WebSocket = webSocket;
            }
        }
    }
    
}
