using DataLayer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using MyLibrary;
using Website.Services;
using Website.ViewModels;


namespace Website.Controllers.SignInUpOut
{
    public class SignUpController : Controller
    {

        private readonly ApplicationContext _context;
        
        private readonly SimpleLogger _logger;
        private readonly RegistrationService _registrationService;

        public SignUpController(ApplicationContext context, 
            SimpleLogger logger, RegistrationService registrationService)
        {
            _context = context;
            _logger = logger;
            _registrationService = registrationService;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _registrationService.Register(model, HttpContext.Request.Host.Value);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("",e.Message);
                }
               
                return RedirectToAction("MyBots", "MyBots");
            }
            return View(model);
        }


        
        [HttpGet]
        public IActionResult EmailCheckSuccess(Guid guid, [FromQuery(Name = "accountId")] int accountId)
        {
            try
            {
                _registrationService.ConfirmEmail(guid, accountId);
                string message = "Поздравляем, ваш email подтверждён";
                return RedirectToAction("Success", "StaticMessage", new { message });
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.WARNING, Source.WEBSITE, 
                    "При переходе на страцицу с гуидом для завершения регистрации произошла ошибка.", accountId,exception);

                string errorMessage = "Что-то пошло не так";
                return RedirectToAction("Failure", "StaticMessage", new { message = errorMessage });
            }
        }
    }
}