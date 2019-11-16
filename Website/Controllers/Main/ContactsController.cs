using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.SignInUpOut
{
    public class ContactsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}