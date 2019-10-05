using System;
using System.Globalization;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Website.Controllers.Main
{
    [Authorize]
    public class MainController : Controller
    {
        readonly ApplicationContext _context;
        private readonly IStringLocalizer<MainController> _localizer;

        public MainController(ApplicationContext context, IStringLocalizer<MainController> localizer)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            _localizer = localizer;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var www = HttpContext.Request.Headers ;
            
            Console.WriteLine($"CurrentCulture:{CultureInfo.CurrentCulture.Name}, CurrentUICulture:{CultureInfo.CurrentUICulture.Name}");
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
