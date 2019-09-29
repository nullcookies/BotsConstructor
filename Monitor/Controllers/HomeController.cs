using Microsoft.AspNetCore.Mvc;
using Monitor.TelegramAgent;

namespace Monitor.Controllers
{
    public class HomeController:Controller
    {
        private readonly MyTelegramAgent _agent;

        public HomeController(MyTelegramAgent agent)
        {
            _agent = agent;
        }

     
        public IActionResult Index(string code)
        {
            _agent.MakeAuthAsync(code).Wait();
            return Content("готово");
        }
    }
}