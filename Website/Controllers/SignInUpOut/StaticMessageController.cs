using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.SignInUpOut
{
    public class StaticMessageController : Controller
    {
                
        [HttpGet]
        [HttpPost]
        public IActionResult Success(string message)
        {
            ViewData["message"] = message;
            return View();

        }

        [HttpGet]
        [HttpPost]
        public IActionResult Failure(string message)
        {
            ViewData["message"] = message;
            return View();

        }
    }
}