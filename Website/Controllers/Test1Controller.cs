using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Website.Controllers
{
    public class Test1Controller : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Test1Controller(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return StatusCode(404);
            }

            return View();
        }
        
    }
}