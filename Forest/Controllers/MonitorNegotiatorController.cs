using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Forest.Controllers
{
    public class MonitorNegotiatorController:Controller
    {
        private readonly ApplicationContext _contextDb;

        public MonitorNegotiatorController(ApplicationContext contextDb)
        {
            _contextDb = contextDb;
        }

        [HttpPost]
        public IActionResult Ping()
        {
            return Ok();
        }

//        [HttpPost]
//        public IActionResult BotIsHere(int botId)
//        {
//            JObject jsonAnswer;
//            
//            var bot = _contextDb.Bots.Find(botId);
//            if (bot == null)
//            {
//                jsonAnswer = new JObject {{"success", false}, {"failMessage", "В базе данных нет бота с таким id"}};
//                return Json(jsonAnswer);
//            }
//            
//            string botUsername = bot.BotName;
//            bool botIsHere = BotsStorage.BotsDictionary.Keys.Contains(botUsername);
//
//            if (botIsHere)
//                jsonAnswer = new JObject {{"success", true}};
//            else
//                jsonAnswer = new JObject {{"success", false}, {"failMessage", "Такого бота нет в этом лесу"}};
//
//            return Json(jsonAnswer);
//        }
    }
}