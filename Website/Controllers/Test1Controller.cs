using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Website.Controllers
{
    public class Test1Controller : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            //int accountId = (int) HttpContext.Items["acoountId"];
            return View();
        }
        
    }
}