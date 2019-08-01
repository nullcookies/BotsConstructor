using DataLayer.Models;
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
using Website.Models;
using Website.Other;
using Website.Services;

namespace Website.Controllers
{
	[Authorize]
    public class OrdersController : Controller
    {
        ApplicationContext _contextDb;
        OrdersCountNotificationService _ordersCounter;

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

            int accountId = 0;

            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
            }catch{
                return;
            }
            
            var isSocketRequest = HttpContext.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var socketFinishedTcs = new TaskCompletionSource<object>();

                await _ordersCounter.RegisterInNotificationSystem(accountId, webSocket, socketFinishedTcs);

                await socketFinishedTcs.Task;

            }
            else
            {
                HttpContext.Response.StatusCode = 404;
            }
        }

        private async Task RegisterInNotificationSystem(WebSocket webSocket)
        {
            int ordersCount = 45;
           
            JObject JObj = new JObject
            {
                { "ordersCount", ordersCount}
            };

            string jsonString = JsonConvert.SerializeObject(JObj);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var arraySegment = new ArraySegment<byte>(bytes);
            await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

            Thread.Sleep(2000);



        }



        public IActionResult Orders2(int page=1)
		{
            int accountId = 0;

            try{
                accountId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
            }catch{
                return RedirectToAction("Account", "Login");
            }

            ViewData["currentPage"] = page;

			//Общее кол-во записей
			int count = _contextDb.Orders.Where(_order => _order.Bot.OwnerId == accountId).Count();
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
            int ownerId = 0;
			try
			{
				ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Account", "Login");
			}

			Order order = _contextDb.Orders.Where(_order => _order.Id == orderId &&
			_order.Bot.OwnerId == ownerId).SingleOrDefault();

			if (order != null)
			{
				_contextDb.Orders.Remove(order);
				_contextDb.SaveChanges(); // Не слишком ли часто сохранение?
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
            int ownerId = 0;
			try
			{
				ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Account", "Login");
			}

			Order order = _contextDb.Orders.Where(_order => _order.Id == orderId &&
			_order.Bot.OwnerId == ownerId).SingleOrDefault();

			if (order != null)
			{
				bool correct = true;
				if(statusId != null)
				{
					OrderStatus status = _contextDb.OrderStatuses.Where(_status => _status.Id == statusId &&
					_status.Group.OwnerId == ownerId).SingleOrDefault();
					correct = status != null;
				}

				if (correct)
				{
					order.OrderStatusId = statusId;
					_contextDb.Update(order);
					_contextDb.SaveChanges(); // Не слишком ли часто сохранение?
					return GetNewOrdersCount();
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
			int ownerId = 0;
			try
			{
				ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Account", "Login");
			}


			int ordersCount = _contextDb.Orders.Where(_order => _order.Bot.OwnerId == ownerId && _order.OrderStatusId == null).Count();

			return Content(ordersCount.ToString());
		}

		[HttpPost]
		public IActionResult GetVariables()
		{
			int ownerId = 0;
			try
			{
				ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Account", "Login");
			}


			var allStatuses = new Dictionary<int, (string name, string message)>();
			var groups = new Dictionary<int, (string name, List<int> statuses)>();

			var statusGroups = _contextDb.OrderStatusGroups.Where(_group => _group.OwnerId == ownerId);
			foreach (var statusGroup in statusGroups) // группы статусов меняются редко, может, нужно их сохранять?
			{
				int groupId = statusGroup.Id;
				groups.Add(groupId, (statusGroup.Name, new List<int>()));
				var statuses = _contextDb.OrderStatuses.Where(_status => _status.GroupId == groupId);
				foreach (var status in statuses)
				{
					groups[groupId].statuses.Add(status.Id);
					allStatuses.Add(status.Id, (status.Name, status.Message));
				}
			}

			var bots = _contextDb.Bots.Where(_bot => _bot.OwnerId == ownerId).
				ToDictionary(_bot => _bot.Id, _bot => new { Name = _bot.BotName, _bot.Token });

			var items = _contextDb.Items.Where(_item => _item.Bot.OwnerId == ownerId).ToDictionary(
				_item => _item.Id, _item => new
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
			int ownerId = 0;
			try
			{
				ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId");
			}
			catch
			{
				return RedirectToAction("Account", "Login");
			}


			int startPosition = (page - 1) * pageSize;

			var orders = _contextDb.Orders.Where(_order => _order.Bot.OwnerId == ownerId).
				OrderByDescending(_order => _order.DateTime).Skip(startPosition).Take(pageSize).Select(_order =>
				new {
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
					ItemsIds = _cont.Items.Select(_item => new { _item.Id, _item.Count }).ToArray(),
					Texts = _cont.Texts.Select(_text => _text.Text).ToArray(),
					Files = _cont.Files.Select(_file => new { _file.FileId, _file.PreviewId, _file.Description }).ToArray()
				}).ToList();

			for (int i = 0; i < containers.Count; i++)
			{
				AddContainers(containers[i].Id);
			}

			return Json(new { orders, containers });

			void AddContainers(int parentId)
			{
				var children = _contextDb.Inventories.Where(_cont => _cont.ParentId == parentId).
					Select(_cont => new
					{
						_cont.Id,
						_cont.ParentId,
						ItemsIds = _cont.Items.Select(_item => new { _item.Id, _item.Count }).ToArray(),
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



            int ownerId = 0;
            try{
                ownerId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("В cookies не найден accountId.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            int senderId = _contextDb.Orders.Where(_order => _order.Id == orderId && _order.Bot.OwnerId == ownerId).
				Select(_order => _order.SenderId).SingleOrDefault();

			if(senderId == default(int))
			{
				Response.StatusCode = 403;
				return Content("Incorrect order ID.");
			}

			//BotForest.SendMessage(senderId, text);

			return new EmptyResult();
		}
	}
}
