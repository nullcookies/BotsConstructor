using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using MyLibrary;
using Website.Other;
using Website.Other.Filters;

namespace Website.Controllers
{
    public class ModeratorManagementController : Controller
    {
        readonly ApplicationContext _contextDb;
        readonly StupidLogger _logger;

        public ModeratorManagementController(ApplicationContext contextDb, StupidLogger logger)
        {
            _contextDb = contextDb;
            _logger = logger;
        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult Index(int botId)
        {
            return View();
        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult InviteANewModerator( int botId,  string email)
        {
			if(email == null)
			{
				JObject jObject = new JObject()
				{
					{ "success", false },
					{ "failMessage", "Для добавления модератора нужно ввести почту." }
				};

				return Json(jObject);
			}

            Account searchedAccount = _contextDb.Accounts.SingleOrDefault(_acc => _acc.Email == email.Trim());

            //себя нельзя сделать модератором своего бота
            //просто получится самоспам
            if (searchedAccount != null && searchedAccount.Id == Stub.GetAccountIdFromCookies(HttpContext))
            {
                JObject jObject = new JObject()
                {
                    { "success", false },
                    { "failMessage", "Вы не можете добавить себя в список модераторов своего бота." }
                };

                return Json(jObject);
            }


            if (searchedAccount != null)
            {
                //TODO отправить запрос стать модератором на почту

                bool accountIsModeratingThisBot = _contextDb.Moderators.Any(_mo => _mo.AccountId == searchedAccount.Id && _mo.BotId == botId);

                JObject jObject;

                if (accountIsModeratingThisBot)
                {

                    jObject= new JObject()
                    {
                        { "success", false },
                        { "failMessage", "Аккаунт уже модерирует этого бота." }
                    };

                    return Json(jObject);
                }
                else
                {
                    _contextDb.Moderators.Add(new Moderator() { AccountId = searchedAccount.Id, BotId = botId });

                    _contextDb.SaveChanges();

                    jObject = new JObject()
                    {
                        { "success", true },
                    };
                }
              

                return Json(jObject);

            }
            else
            {
                JObject jObject = new JObject()
                {
                    { "success", false },
                    { "failMessage", "Такого аккаунта не существует." }
                };

                return Json(jObject);
            }
            
        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetListOfModerators(int botId)
        {
            //как из этого сделать одну оперецию?
            List<int> moderatorIds = _contextDb.Moderators.Where(_mo => _mo.BotId == botId).Select(_mo=>_mo.AccountId).ToList();
            var accountsInfo = _contextDb.Accounts.Where(_acc => moderatorIds.Contains(_acc.Id)).Select(_acc=>new {_acc.Name, _acc.Email, _acc.Id }).ToList();

            JArray jArray = new JArray();

            foreach (var account in accountsInfo)
            {
                JObject accountJson = (JObject) JsonConvert.DeserializeObject(  JsonConvert.SerializeObject(account));
                jArray.Add(accountJson);
            }
            
            JObject jObject = new JObject()
            {
                { "success", true},
                { "moderators", jArray}
            };

            return Json(jObject);
        }

        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult RemoveModerator(int botId, int accountId)
        {
            //поиск по таблице модераторов
            Moderator moderator = _contextDb.Moderators.SingleOrDefault(_mo => _mo.AccountId == accountId);
            JObject answer = null;

            if (moderator != null)
            {
                _contextDb.Moderators.Remove(moderator);
                _contextDb.SaveChanges();

                answer = new JObject()
                {
                    { "success", true }
                };
            }
            else
            {
                _logger.Log(LogLevelMyDich.USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT, Source.WEBSITE, $"Сайт. Аккаунт " +
                    $"{Stub.GetAccountIdFromCookies(HttpContext)} пытается удалить из списка модераторов" +
                    $"аккаунт с id = {accountId}. Но удаляемый аккаунт и так не модерирует этого бота.");

                answer = new JObject()
                {
                    {  "success", false},
                    { "failMessage", "Аккаунт с таким id не является модератором этого бота"}
                };

            }

            return Json(answer);
        }
    }
}
