using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitor.Services;


namespace Monitor.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private DiagnosticService _diagnosticService;

        public HomeController(DiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService;
        }

        public IActionResult Index()
        {
            var statistics = _diagnosticService.GetTargetsStatistics();
            
            ViewData["targets"] = statistics; 
            return View();
        }
    }
}