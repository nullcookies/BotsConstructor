using DataLayer.Models;
using DataLayer.Services;
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


//Вебсокет, похоже портится при выходе потока


namespace Website.Services
{
    public class OrdersCountNotificationService
    {
        ApplicationContext _contextDb;
        StupidLogger _logger;

        public OrdersCountNotificationService(IConfiguration configuration, StupidLogger _logger)
        {
            this._logger = _logger;

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

            PeriodicFooAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        }



        public async Task<bool> PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {                
                await SendNotificationsOfNewOrders();                
                await Task.Delay(interval, cancellationToken);                
            }
        }
    
        private async Task SendNotificationsOfNewOrders()
        {
            List<Order> allOrders = null;
            
            //Это помогает избежать падения?
            lock (lockObj)
            {
                allOrders = _contextDb.Orders.ToList();
            }


            //Перебираю все заказы
            for (int i = 0; i < allOrders.Count; i++)
            {
                Console.WriteLine($"i={i}");
                Order order = allOrders[i];


                Console.WriteLine($"order.OrderStatusId  = {order.OrderStatusId }");
                //Это новый заказ
                if (order.OrderStatusId == null)
                {

                    int botId = order.BotId;
                    Console.WriteLine($"Это новый заказ. botId= {botId}");

                    //У нас есть аккаунты, которые подписаны на этого бота
                    if (_dict_botId_accountIds.ContainsKey(botId))
                    {
                        Console.WriteLine($"//У нас есть аккаунты, которые подписаны на этого бота");

                        List<int> accountIds = _dict_botId_accountIds[botId];

                        //Инкремент счётчика аккаунта
                        for (int j = 0; j < accountIds.Count; j++)
                        {
                            int accountId = accountIds[j];
                            Console.WriteLine($"accountId = {accountId }");
                            Console.WriteLine($"j = {j}");

                            _dict_accountId_WebSocket[accountId].OrdersCount++;
                        }
                    }
                }
            }

           

            //Отправка новых чисел в счётчике
            foreach (var key in _dict_accountId_WebSocket.Keys)
            {
                int ordersCount = _dict_accountId_WebSocket[key].OrdersCount;

                for (int k = 0; k < _dict_accountId_WebSocket[key].WebSockets.Count; k++)
                {
                    WebSocket webSocket = _dict_accountId_WebSocket[key].WebSockets[k];

                    JObject JObj = new JObject
                        {
                            { "ordersCount", ordersCount}
                        };

                    string jsonString = JsonConvert.SerializeObject(JObj);
                    var bytes = Encoding.UTF8.GetBytes(jsonString);
                    var arraySegment = new ArraySegment<byte>(bytes);
               
                    await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }

                _dict_accountId_WebSocket[key].OrdersCount = 0;
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


        

        public void RegisterInNotificationSystem(int accountId, WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
        {

            //На всякий случай
            //Это вообще как-то помогает?
            lock (lockObj)
            {
                if (_dict_accountId_WebSocket.ContainsKey(accountId))
                {
                    _dict_accountId_WebSocket[accountId].WebSockets.Add(webSocket);
                }
                else
                {
                    //false при
                    //кончилась память
                    //такой ключ уже есть
                    //key == null
                    bool addIsOk = _dict_accountId_WebSocket.TryAdd(accountId, new OrdersCountModel() { WebSockets=new List<WebSocket>() { webSocket } } );

                    if (!addIsOk)
                    {
                        _logger.Log(LogLevelMyDich.ERROR, $"Сайт. Сервис подсчёта заказов. Не удалось добавить webSocket для аккаунта accountId={accountId},");
                    }
                }



                //На каких ботов подписать аккаунт?

                //Боты, которые принадлежат аккаунту
                List<int> the_bot_ids_on_which_the_account_is_signed =
                    _contextDb.Bots.Where(_bot => _bot.OwnerId == accountId).Select(_bot => _bot.Id).ToList();

                //TODO Боты, к которым аккаунт имеет доступ(модератор)

                //Для всех ботов на которых аккаунт подписан
                for (int i = 0; i < the_bot_ids_on_which_the_account_is_signed.Count; i++)
                {
                    int botId = the_bot_ids_on_which_the_account_is_signed[i];

                    List<int> accounts_that_are_subscribed_to_bot = null;

                    _dict_botId_accountIds.TryGetValue(botId, out accounts_that_are_subscribed_to_bot);

                    //Добавить к списку подписчиков на бота текущий аккаунт
                    if (accounts_that_are_subscribed_to_bot != null
                        && accounts_that_are_subscribed_to_bot.Count > 0)
                    {
                        if (!accounts_that_are_subscribed_to_bot.Contains(accountId))
                        {
                            accounts_that_are_subscribed_to_bot.Add(accountId);
                        }
                        else
                        {
                            _logger.Log(LogLevelMyDich.INFO, "К словарю (accountId, List<WebSocket> ) добавлен сокет для аккаунта, который уже существует. Значит открыто несколько вкладок.");
                        }
                    }
                    else
                    {
                        _dict_botId_accountIds.TryAdd(botId, new List<int>() { accountId });

                    }
                }
            }
                       
        }


        object lockObj = new object();

        private class OrdersCountModel
        {
            public int OrdersCount;
            public List< WebSocket > WebSockets;

        }
    }
    
}
