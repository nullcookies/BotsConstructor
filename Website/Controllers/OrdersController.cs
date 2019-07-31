using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataLayer.Models;
using Website.Other;

namespace Website.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        ApplicationContext context;

        public OrdersController(ApplicationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		//Кол-во записей на странице
		const int pageSize = 12;

        public IActionResult Orders2(int page=1)
		{

            int accountId = 0;

            try{
                accountId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }

            ViewData["currentPage"] = page;

			//Общее кол-во записей
			int count = context.Orders.Where(_order => _order.Bot.OwnerId == accountId).Count();
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
           

            try{
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            Order order = context.Orders.Where(_order => _order.Id == orderId &&
			_order.Bot.OwnerId == ownerId).SingleOrDefault();

			if (order != null)
			{
				context.Orders.Remove(order);
				context.SaveChanges(); // Не слишком ли часто сохранение?
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
           

            try{
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }

            Order order = context.Orders.Where(_order => _order.Id == orderId &&
			_order.Bot.OwnerId == ownerId).SingleOrDefault();

			if (order != null)
			{
				bool correct = true;
				if(statusId != null)
				{
					OrderStatus status = context.OrderStatuses.Where(_status => _status.Id == statusId &&
					_status.Group.OwnerId == ownerId).SingleOrDefault();
					correct = status != null;
				}

				if (correct)
				{
					order.OrderStatusId = statusId;
					context.Update(order);
					context.SaveChanges(); // Не слишком ли часто сохранение?
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
            try{
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            int ordersCount = context.Orders.Where(_order => _order.Bot.OwnerId == ownerId && _order.OrderStatusId == null).Count();

			return Content(ordersCount.ToString());
		}

		[HttpPost]
		public IActionResult GetVariables()
		{


            int ownerId = 0;
            try{
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            var allStatuses = new Dictionary<int, (string name, string message)>();
			var groups = new Dictionary<int, (string name, List<int> statuses)>();

			var statusGroups = context.OrderStatusGroups.Where(_group => _group.OwnerId == ownerId);
			foreach (var statusGroup in statusGroups) // группы статусов меняются редко, может, нужно их сохранять?
			{
				int groupId = statusGroup.Id;
				groups.Add(groupId, (statusGroup.Name, new List<int>()));
				var statuses = context.OrderStatuses.Where(_status => _status.GroupId == groupId);
				foreach (var status in statuses)
				{
					groups[groupId].statuses.Add(status.Id);
					allStatuses.Add(status.Id, (status.Name, status.Message));
				}
			}

			var bots = context.Bots.Where(_bot => _bot.OwnerId == ownerId).
				Select(_bot => new { _bot.Id, _bot.BotName }).ToDictionary(_bot => _bot.Id, _bot => _bot.BotName);

			var items = context.Items.Where(_item => _item.Bot.OwnerId == ownerId).ToDictionary(
				_item => _item.Id, _item => new
				{
					_item.Name,
					_item.Value
				});

			return Json(new { statuses = allStatuses, statusGroups = groups, bots, items });
		}

		private static readonly long unixEpochTicks = DateTime.UnixEpoch.Ticks;

		[HttpPost]
		public IActionResult GetOrders(int page=1)
		{

            int ownerId = 0;
            try{
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            int startPosition = (page - 1) * pageSize;

			var orders = context.Orders.Where(_order => _order.Bot.OwnerId == ownerId).
				OrderByDescending(_order => _order.DateTime).Skip(startPosition).Take(pageSize).Select(_order =>
				new {orderId = _order.Id,
					botId = _order.BotId,
					sender = _order.SenderNickname,
					mainContainerId = _order.ContainerId,
					statusGroupId = _order.OrderStatusGroupId,
					statusId = _order.OrderStatusId,
					dateTime = (_order.DateTime.Ticks - unixEpochTicks) / 10000
				}).ToArray();

			var containers = context.Inventories.
				Join(orders, _cont => _cont.Id, _order => _order.mainContainerId,
				(_cont, _order) => new
				{
					_cont.Id,
					_cont.ParentId,
					ItemsIds = _cont.Items.Select(_item => new { _item.Id, _item.Count }).ToArray(),
					Texts = _cont.Texts.Select(_text => _text.Text).ToArray(),
					FilesIds = _cont.Files.Select(_file => _file.FileId).ToArray()
				}).ToList();

			for (int i = 0; i < containers.Count; i++)
			{
				AddContainers(containers[i].Id);
			}

			return Json( new { orders, containers });

			void AddContainers(int parentId)
			{
				var children = context.Inventories.Where(_cont => _cont.ParentId == parentId).
					Select(_cont => new
					{
						_cont.Id,
						_cont.ParentId,
						ItemsIds = _cont.Items.Select(_item => new { _item.Id, _item.Count }).ToArray(),
						Texts = _cont.Texts.Select(_text => _text.Text).ToArray(),
						FilesIds = _cont.Files.Select(_file => _file.FileId).ToArray()
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
                ownerId = Stub.GetAccountIdByHttpContext(HttpContext, context) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Account", "Login");
            }


            int senderId = context.Orders.Where(_order => _order.Id == orderId && _order.Bot.OwnerId == ownerId).
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
