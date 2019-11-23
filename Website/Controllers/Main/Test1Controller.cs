using DataLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;

namespace Website.Controllers.Main
{
    public class Test1Controller : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private SimpleLogger _logger;

        public Test1Controller(IHostingEnvironment hostingEnvironment, SimpleLogger logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return StatusCode(404);
            }

            //int accountId = (int)HttpContext.Items["accountId"];

            return View();
        }
        
    }
}