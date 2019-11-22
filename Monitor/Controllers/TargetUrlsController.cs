using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitor.Models;
using Monitor.Services;


namespace Monitor.Controllers
{
    [Authorize]
    public class TargetUrlsController : Controller
    {
        private readonly DiagnosticService _diagnosticService;

        public TargetUrlsController(DiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService;
        }

        [HttpGet]
        public IActionResult AddUrl()
        {
            return View("Index");
        }
        [HttpPost]
        public IActionResult AddUrl(UrlModel model)
        {
            string errorMessage="";
            if (_diagnosticService.TryAddUrl(model.Url, ref errorMessage))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return Content($"Не удалось добавить ссылку для опроса. Причина {errorMessage}");
            }
        }
    }
}