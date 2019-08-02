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


namespace Website.Services
{

    /// <summary>
    /// Отдаёт актуальное число заказов для аккаунта
    /// 
    /// </summary>
    public class OrdersCountNotificationService
    {
        StupidLogger _logger;
        ApplicationContext _contextDb {
            get
            {
                return _dbContextWrapper.GetDbContext();
            }
        }
        DbContextWrapper _dbContextWrapper;

        public OrdersCountNotificationService(IConfiguration configuration, StupidLogger _logger)
        {
            this._logger = _logger;

            _dbContextWrapper = new DbContextWrapper(configuration);
          
            PeriodicFooAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        //Периодический запуск
        public async Task<bool> PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {                
                await SendNotificationsOfNewOrders();                
                await Task.Delay(interval, cancellationToken);                
            }
        }

        /// <summary>
        /// Раз в интервал выбирает всю таблицу заказов и отправляет 
        /// всем подписавшимся клиентам актуальное кол-во заказов есди оно изменилось
        /// </summary>
        private async Task SendNotificationsOfNewOrders()
        {
            List<Order> allOrders = null;
            
            lock (lockObj)
            {
                allOrders = _contextDb.Orders.ToList();                
            }


            //Перебираю все заказы
            for (int i = 0; i < allOrders.Count; i++)
            {
                Order order = allOrders[i];
                //Это новый заказ
                if (order.OrderStatusId == null)
                {
                    int botId = order.BotId;                    

                    //У нас есть аккаунты, которые подписаны на этого бота
                    if (_dict_botId_accountIds.ContainsKey(botId))
                    {                    
                        List<int> accountIds = _dict_botId_accountIds[botId];

                        //По всем подписанным аккаунтам
                        //Инкремент счётчика аккаунта
                        for (int j = 0; j < accountIds.Count; j++)
                        {
                            int accountId = accountIds[j];
                            List<UserWebSocketPair> userSession_Websocket = _dict_accountId_WebSocket[accountId];
                            for (int q = 0; q < userSession_Websocket.Count; q++)
                            {
                                UserWebSocketPair pair = userSession_Websocket[q];
                                pair.OrdersCount++;

                            }                            
                        }
                    }
                }
            }

           

            //Отправка новых чисел в счётчике
            foreach (int accountId in _dict_accountId_WebSocket.Keys)
            {
                List<UserWebSocketPair> userSession_websockets = _dict_accountId_WebSocket[accountId];

                for (int w = 0; w < userSession_websockets.Count; w++)
                {
                    UserWebSocketPair userSession_websocket = userSession_websockets[w];

                    //Если значение поменялось
                    if (userSession_websocket.OrdersCount != userSession_websocket.OrdersCountOld)
                    {
                        //Отправка нового значения
                        WebSocket webSocket = userSession_websocket.WebSocket;
                        int ordersCount = userSession_websocket.OrdersCount;

                        JObject JObj = new JObject
                        {
                            { "ordersCount", ordersCount}
                        };

                        string jsonString = JsonConvert.SerializeObject(JObj);
                        var bytes = Encoding.UTF8.GetBytes(jsonString);
                        var arraySegment = new ArraySegment<byte>(bytes);
                                                
                        await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                    //Обновление и сброс количества заказов
                    userSession_websocket.OrdersCountOld = userSession_websocket.OrdersCount;
                    userSession_websocket.OrdersCount = 0;

                }

            }
        }



        /// <summary>
        /// Словарь для того, чтобы знать куда отправлять уведомление
        /// </summary>
        private static ConcurrentDictionary<int, List<UserWebSocketPair>> _dict_accountId_WebSocket = new ConcurrentDictionary<int, List<UserWebSocketPair>>();

        /// <summary>
        /// Словарь для того, чтобы знать кому отправлять уведомления
        /// </summary>
        private static ConcurrentDictionary<int, List<int>> _dict_botId_accountIds = new ConcurrentDictionary<int, List<int>>();


        
        /// <summary>
        /// Подписывает аккаунт на уведомления всех доступных ботов
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="webSocket"></param>
        /// <param name="socketFinishedTcs"></param>
        public void RegisterInNotificationSystem(int accountId, WebSocket webSocket)
        {

            //На всякий случай
            //Это вообще как-то помогает?
            lock (lockObj)
            {
                UserWebSocketPair pair = new UserWebSocketPair() { OrdersCountOld = 0, OrdersCount = 0, WebSocket = webSocket };

                //Если уже есть такой аккаунт(другие вкладки или несколько компьютеров)
                if (_dict_accountId_WebSocket.ContainsKey(accountId))
                {
                    _dict_accountId_WebSocket[accountId].Add(pair );
                }
                else
                {
                    //false при
                    //кончилась память
                    //такой ключ уже есть
                    //key == null
                    bool addIsOk = _dict_accountId_WebSocket.TryAdd(accountId, 
                        new List<UserWebSocketPair>()
                        {
                           pair
                        } 
                    );

                    if (!addIsOk)
                    {
                        _logger.Log(LogLevelMyDich.ERROR, $"Сайт. Сервис подсчёта заказов. Не удалось добавить webSocket для аккаунта accountId={accountId},");
                    }
                }



                //На каких ботов подписать аккаунт?

                //Боты, которые принадлежат аккаунту
                List<int> the_bot_ids_on_which_the_account_is_signed =
                    _contextDb.Bots.Where(_bot => _bot.OwnerId == accountId).Select(_bot => _bot.Id).ToList();

                //TODO Боты, к которым аккаунт имеет доступ (модератор)

                //Для всех ботов на которые аккаунт подписан
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

        /// <summary>
        /// Хранит количество заказов для счётчика на сайте
        /// и вебсокет для каждой вкладки браузера
        /// </summary>
        private class UserWebSocketPair
        {
            public int OrdersCount;
            public int OrdersCountOld;
            public WebSocket WebSocket;
        }
        /// Зачем для каждой вкладки? 
        /// Вообще незачем, но раз упарываться по оптимизации сети,
        /// то можно представить такую ситуацию:
        /// 
        /// на 10 компьютерах открыто по 1 вкладке с одного аккаунта
        /// на всех 10-ти компьютерах показывается число 3 в количестве заказов
        /// 
        /// и тут хоба и включается 11-ый пользователь под тем же аккаунтом
        /// для него в переменной хранится кол-во заказов 0
        /// 
        /// при новой рассылке уведомлений Json с кол-вом заказов отправится только 
        /// 11-тому, а не всем вкладкам под этим аккаунтом
        /// 



    }

    public class DbContextWrapper
    {
        private readonly string _connextionString;

        public DbContextWrapper(IConfiguration configuration)
        {

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
                _connextionString = configuration.GetConnectionString("PostgresConnectionDevelopment");
            else
                _connextionString = configuration.GetConnectionString("PostgresConnectionLinux");


        }

        public ApplicationContext GetDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql(_connextionString)
                .Options);
        }
    }

}
