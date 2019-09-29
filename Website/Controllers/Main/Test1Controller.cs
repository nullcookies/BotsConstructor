using DataLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.Main
{
    public class Test1Controller : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private StupidLogger _logger;

        public Test1Controller(IHostingEnvironment hostingEnvironment, StupidLogger logger)
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