//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Monitor.ViewModels;
//using Website.TelegramAgent;
//
//namespace Website.Controllers
//{
//    public class TelegramAgentSettingsController:Controller
//    {
//        private readonly MyTelegramAgent _telegramAgent;
//
//        public TelegramAgentSettingsController(MyTelegramAgent telegramAgent)
//        {
//            _telegramAgent = telegramAgent;
//        }
//
//        [HttpGet]
//        public IActionResult Index()
//        {
//            return View();
//        }
//        [HttpGet]
//        public async Task<IActionResult> SendCode()
//        {
//            try
//            {
//                await _telegramAgent.SendCodeAsync();
//            }
//            catch (Exception eee)
//            {
//                return Content("не работает. "+eee.Message);
//            }
//            
//            return Content("работает");
//        }
//        
//        [HttpPost]
//        public async Task<IActionResult> Index(TelegtamAgentActivationCode  model )
//        {
//            try
//            {
//                await _telegramAgent.MakeAuthAsync(model.Code);
//            }
//            catch (Exception eee)
//            {
//                return Content("не работает. "+eee.Message);
//            }
//
//            return Content("работает");
//        }
//
//
//        public IActionResult AddFile(IFormFile uploadedFile)
//        {
//            return Ok();
//        }
//    }
//}