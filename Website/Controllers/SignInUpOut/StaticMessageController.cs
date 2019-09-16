using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.SignInUpOut
{
    public class StaticMessageController : Controller
    {
                
        [HttpGet]
        public IActionResult Success(string message)
        {
            ViewData["message"] = message;
            return View();

        }
    }
}