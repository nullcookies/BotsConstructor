using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataLayer;
using MyLibrary;
using Website.Models;
using Website.Other;
using Website.Services;

namespace Website.Controllers
{
	[Authorize]
    public class OrdersController : Controller
    {
        readonly ApplicationContext _contextDb;
        readonly OrdersCountNotificationService _ordersCounter;

        public OrdersController(ApplicationContext _contextDb, OrdersCountNotificationService _ordersCounter)
        {
            this._contextDb = _contextDb ?? throw new ArgumentNullException(nameof(_contextDb));
            this._ordersCounter = _ordersCounter;
		}

		//Кол-во записей на странице
		const int pageSize = 12;

        [HttpGet]
        public async Task SetWebsocketOrdersCount()
        {

            int accountId = (int)HttpContext.Items["accountId"];


            var isSocketRequest = HttpContext.WebSockets.IsWebSocketRequest;
            

            if (isSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var socketFinishedTcs = new TaskCompletionSource<object>();

                _ordersCounter.RegisterInNotificationSystem(accountId, webSocket);

                await socketFinishedTcs.Task;

            }
            else
            {
                HttpContext.Response.StatusCode = 404;
            }
        }


        public IActionResult Orders2(int page=1)
		{
            int accountId = 0;

            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
            }catch{
                return RedirectToAction("Login", "SignIn");
            }

            ViewData["currentPage"] = page;

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();

            //Общее кол-во записей
            //Собственник или модератор
            int count = _contextDb.Orders.Count(_order => _order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.Bot.Id));

			//ViewData["pageSize"] = pageSize;
			ViewData["pagesCount"] = (count - 1) / pageSize + 1;

			PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            IndexViewModelPagination viewModel = new IndexViewModelPagination
            {
                PageViewModel = pageViewModel
            };

            return View(viewModel);
        }
		
        [HttpDelete]
		public IActionResult RemoveOrder(int orderId)
		{
            int accountId = 0;
			try
			{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Login", "SignIn");
			}

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();


            //Собственник или модератор
            Order order = _contextDb.Orders.SingleOrDefault(_order => _order.Id == orderId &&
                                                                      (_order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.Bot.Id)));

			if (order != null)
			{
				_contextDb.Orders.Remove(order);
				_contextDb.SaveChanges();
				return GetNewOrdersCount();
			}
			else
			{
				Response.StatusCode = 403;
				return Content("Incorrect order ID.");
			}
		}

		[HttpPost]
		public IActionResult ChangeOrderStatus(int orderId, int? statusId)
		{
            int accountId = 0;
			try
			{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Login", "SignIn");
			}

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();


            //Собственник или модератор
            Order order = _contextDb.Orders.SingleOrDefault(_order => _order.Id == orderId &&
                                                                      (_order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.Bot.Id)));

			if (order != null)
			{
				bool correct = true;
				if(statusId != null)
				{

                    bool moderator = idsOfModeratedBots.Contains(order.BotId);


                    OrderStatus status = _contextDb.OrderStatuses.SingleOrDefault(_status => _status.Id == statusId && !_status.IsOld &&
                                                                                             (_status.Group.OwnerId == accountId || moderator));
					correct = status != null;
				}
				else
				{
					return GetNewOrdersCount();
				}

				if (correct)
				{
					order.OrderStatusId = statusId;
					_contextDb.Update(order);
					_contextDb.SaveChanges();
					var orderInfo = _contextDb.Orders.Where(_order => _order.Id == orderId).Select(_order => new { _order.SenderId, _order.OrderStatus.Message, _order.Bot.Token }).SingleOrDefault();
					string url = "https://api.telegram.org/bot" + orderInfo.Token + "/sendMessage";
					string data = "chat_id=" + orderInfo.SenderId + "&text=" + System.Web.HttpUtility.UrlEncode(orderInfo.Message);
					var sending = Stub.SendPostAsync(url, data);
					return sending.ContinueWith((task) => task.IsCompletedSuccessfully ? GetNewOrdersCount() : StatusCode(403, "Can't send message: " + task.Exception?.Message)).Result;
				}
				else
				{
					Response.StatusCode = 403;
					return Content("Incorrect status ID.");
				}
			}
			else
			{
				Response.StatusCode = 403;
				return Content("Incorrect order ID.");
			}
		}

		[HttpPost]
		public IActionResult GetNewOrdersCount()
		{
			int accountId = 0;
			try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}catch{
				return RedirectToAction("Login", "SignIn");
			}

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();


            int ordersCount = _contextDb.Orders.Count(_order => (_order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.Bot.Id)   ) 
                                                                && _order.OrderStatusId == null);

			return Content(ordersCount.ToString());
		}

		[HttpPost]
		public IActionResult GetVariables()
		{
			int accountId = 0;
			try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}catch{
				return RedirectToAction("Login", "SignIn");
			}


			var allStatuses = new Dictionary<int, (string name, string message, bool isOld)>();
			var groups = new Dictionary<int, (string name, List<int> statuses, bool isOld)>();

            //Добавить в список дичи дичь из всех аккаунтов собственников модерируемых ботов
            //List<int> identifier_of_all_accounts_bots_that_I_moderate =

            List<int> all_moderated_bots_ids=
                _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();

            List<int> identifier_of_all_accounts_bots_that_I_moderate =
                _contextDb
                .Bots
                .Where(_bot =>
                all_moderated_bots_ids.Contains(_bot.Id))
                .Select(_bot => _bot.OwnerId)
                .ToList();


            var statusGroups = _contextDb.OrderStatusGroups.Where(_group => 
                    _group.OwnerId == accountId || identifier_of_all_accounts_bots_that_I_moderate.Contains(_group.OwnerId)
                );

			foreach (var statusGroup in statusGroups) // группы статусов меняются редко, может, нужно их сохранять?
			{
				int groupId = statusGroup.Id;
				groups.Add(groupId, (statusGroup.Name, new List<int>(), statusGroup.IsOld));
				var statuses = _contextDb.OrderStatuses.Where(_status => _status.GroupId == groupId);
				foreach (var status in statuses)
				{
					groups[groupId].statuses.Add(status.Id);
					allStatuses.Add(status.Id, (status.Name, status.Message, status.IsOld));
				}
			}

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();


            var bots = _contextDb.Bots.Where(_bot =>
                    _bot.OwnerId == accountId || idsOfModeratedBots.Contains(_bot.Id)
                ).ToDictionary(_bot =>
                _bot.Id, _bot => new
                {
                    Name = _bot.BotName,
                    _bot.Token
                    });

			var items = _contextDb.Items.Where(_item => 
                    _item.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_item.Bot.Id))
                    .ToDictionary(_item => 
                    _item.Id, _item => new
				        {
					        _item.Name,
					        _item.Value
				        });

			return Json(new { statuses = allStatuses, statusGroups = groups, bots, items });
		}

		private static readonly long unixEpochTicks = DateTime.UnixEpoch.Ticks;

		[HttpPost]
		public IActionResult GetOrders(int page = 1)
		{
			int accountId = 0;
			try
			{
				accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Login", "SignIn");
			}


			int startPosition = (page - 1) * pageSize;

            //Добавил модераторов
            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo=>_mo.BotId).ToList();

            var orders = _contextDb.Orders.Where(_order => _order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.BotId)).
            OrderByDescending(_order => _order.DateTime).Skip(startPosition).Take(pageSize).Select(_order =>
            new
            {
                orderId = _order.Id,
                botId = _order.BotId,
                sender = _order.SenderNickname,
                mainContainerId = _order.ContainerId,
                statusGroupId = _order.OrderStatusGroupId,
                statusId = _order.OrderStatusId,
                dateTime = (_order.DateTime.Ticks - unixEpochTicks) / 10000
            }).ToArray();

            var containers = _contextDb.Inventories.
				Join(orders, _cont => _cont.Id, _order => _order.mainContainerId,
				(_cont, _order) => new
				{
					_cont.Id,
					_cont.ParentId,
					ItemsIds = _cont.Items.Select(_item => new { _item.ItemId, _item.Count }).ToArray(),
					Texts = _cont.Texts.Select(_text => _text.Text).ToArray(),
					Files = _cont.Files.Select(_file => new { _file.FileId, _file.PreviewId, _file.Description }).ToArray()
				}).ToList();

            for (var index = 0; index < containers.Count; index++)
            {
                var container = containers[index];
                AddContainers(container.Id);
            }

            return Json(new { orders, containers });

			void AddContainers(int parentId)
			{
				var children = _contextDb.Inventories.Where(_cont => _cont.ParentId == parentId).
					Select(_cont => new
					{
						_cont.Id,
						_cont.ParentId,
						ItemsIds = _cont.Items.Select(_item => new { _item.ItemId, _item.Count }).ToArray(),
						Texts = _cont.Texts.Select(_text => _text.Text).ToArray(),
						Files = _cont.Files.Select(_file => new { _file.FileId, _file.PreviewId, _file.Description }).ToArray()
					});

				containers.AddRange(children);
			}
		}

		[HttpPost]
		public IActionResult SendCustomAnswer(int orderId, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				Response.StatusCode = 403;
				return Content("Incorrect text.");
			}



            int accountId = 0;
            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId.");
            }catch{
                return RedirectToAction("Login", "SignIn");
            }

            List<int> idsOfModeratedBots = _contextDb.Moderators.Where(_mo => _mo.AccountId == accountId).Select(_mo => _mo.BotId).ToList();

            int senderId = _contextDb.Orders.Where(
                _order => _order.Id == orderId && 
                (_order.Bot.OwnerId == accountId || idsOfModeratedBots.Contains(_order.Bot.Id)))
                .Select(_order => _order.SenderId).SingleOrDefault();

			if (senderId == default(int))
			{
				Response.StatusCode = 403;
				return Content("Incorrect order ID.");
			}
			string botToken = _contextDb.Orders.Where(_order => _order.Id == orderId).Select(_order => _order.Bot.Token).SingleOrDefault();
			string url = "https://api.telegram.org/bot" + botToken + "/sendMessage";
			string data = "chat_id=" + senderId + "&text=" + System.Web.HttpUtility.UrlEncode(text);
			return Ok(Stub.SendPostAsync(url, data).Result);
		}
	}
}
