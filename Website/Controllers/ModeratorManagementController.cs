using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Other.Filters;

namespace Website.Controllers
{
    public class ModeratorManagementController : Controller
    {
        ApplicationContext _contextDb;
        StupidLogger _logger;

        public ModeratorManagementController(ApplicationContext contextDb, StupidLogger logger)
        {
            _contextDb = contextDb;
            _logger = logger;
        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Index(int botId)
        {
            //достать из базы весь список модераторов

            return View();
        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult InviteANewModerator( int botId, string emailOrId)
        {
            //определить это email или id

            //поиск аккаунта

            //если есть добавить в таблицу

            //если нет вернуть json с ошибкой

            return StatusCode(500);
        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RemoveModerator(int botId, int accountId)
        {
            //поиск по таблице модераторов

            //если нешёл удалить

            //если нет вернуть ошибку


            return StatusCode(500);
        }
    }
}
