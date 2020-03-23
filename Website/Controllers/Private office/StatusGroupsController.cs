using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Website.Models;
using Website.Other;
using Website.Services;

namespace Website.Controllers
{
    [Authorize]
    public class StatusGroupsController : Controller
    {
        private readonly ApplicationContext _context;

        public StatusGroupsController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!(HttpClientWrapper.GetAccountIdFromCookies(HttpContext) is int accountId))
            {
                return RedirectToAction("Login", "SignIn");
            }

            ViewData["statusGroups"] = _context.OrderStatusGroups.Where(group => group.OwnerId == accountId)
                .Select(group => new
                {
                    group.Id,
                    group.Name,
                    group.IsOld,
                    Statuses = group.OrderStatuses.Select(status => new { status.Id, status.Name, status.Message, status.IsOld }).OrderBy(status => status.Id).ToArray()
                });

            return View();
        }

        [HttpPost]
        public IActionResult SaveStatusGroups(string json)
        {
            //TODO: подумать про безопасность
            if (!(HttpClientWrapper.GetAccountIdFromCookies(HttpContext) is int accountId))
            {
                return RedirectToAction("Login", "SignIn");
            }

            var jArrGroups = JsonConvert.DeserializeObject<JArray>(json);
            var groupList = new List<OrderStatusGroup>(jArrGroups.Count);

            foreach (var jGroup in jArrGroups)
            {
                var group = new OrderStatusGroup
                {
                    OwnerId = accountId,
                    Name = (string)jGroup["name"],
                    IsOld = (bool)jGroup["isOld"]
                };
                groupList.Add(group);

                var jArrStatuses = (JArray)jGroup["statuses"];
                var statuses = new OrderStatus[jArrStatuses.Count];

                for (var i = 0; i < jArrStatuses.Count; i++)
                {
                    var jStatus = jArrStatuses[i];
                    statuses[i] = new OrderStatus()
                    {
                        Name = (string)jStatus["name"],
                        Message = (string)jStatus["message"],
                        IsOld = (bool)jStatus["isOld"]
                    };
                    if ((int?)jStatus["id"] is int statusId)
                    {
                        statuses[i].Id = statusId;
                    }
                }

                if ((int?)jGroup["id"] is int groupId)
                {
                    group.Id = groupId;
                    foreach (var status in statuses)
                    {
                        status.GroupId = groupId;
                        if (status.Id != 0)
                        {
                            _context.OrderStatuses.Update(status);
                        }
                        else
                        {
                            _context.OrderStatuses.Add(status);
                        }
                    }
                    _context.OrderStatusGroups.Update(group);
                }
                else
                {
                    group.OrderStatuses = statuses;
                    _context.OrderStatusGroups.Add(group);
                    _context.OrderStatuses.AddRange(statuses);
                }
            }

            _context.SaveChanges();

            var statusGroupsIds = groupList
                .Select(group => new
                {
                    group.Id,
                    StatusesIds = group.OrderStatuses.Select(status => status.Id).ToArray()
                });

            return Json(statusGroupsIds);
        }

    }
}
