using DataLayer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyLibrary;
using Website.Services;
using Website.ViewModels;


namespace Website.Controllers.SignInUpOut
{
    public class SignUpController : Controller
    {

        private readonly ApplicationContext context;
        private readonly EmailMessageSender emailSender;
        private readonly SimpleLogger logger;

        public SignUpController(ApplicationContext context, 
            EmailMessageSender emailSender, SimpleLogger logger)
        {
            this.context = context;
            this.emailSender = emailSender;
            this.logger = logger;
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
                bool thereIsNoSuchEmailYet = context.EmailLoginInfo
                                                 .FirstOrDefault(a => a.Email == model.Email) == null;

                if (thereIsNoSuchEmailYet)
                {
                    //Отправка сообщения на указанный email, чтобы удостовериться, что он принадлежит этому пользователю
                    if (EmailMessageSender.EmailIsValid(model.Email))
                    {
                        Guid guid = Guid.NewGuid();
                        var link = GetEmailConfirmationLink(model, guid);
                        
                        bool sendIsOk = emailSender.SendEmailCheck(model.Email, model.Name, link);

                        if (sendIsOk)
                        {
                            var tmpAccount = new TemporaryAccountWithUsernameAndPassword()
                            {
                                Name = model.Name,
                                Password = model.Password,
                                Email = model.Email,
                                RegistrationDate = DateTime.UtcNow,
                                Guid = guid
                            };

                            context.TemporaryAccountWithUsernameAndPassword.Add(tmpAccount);
                            context.SaveChanges();
                        }
                    }
                    return RedirectToAction("MyBots", "MyBots");
                }
                else
                {
                    ModelState.AddModelError("", "Аккаунт с таким email уже существует");
                }
            }
            return View(model);
        }

        private static string GetEmailConfirmationLink(RegisterModel model, Guid guid)
        {
            string domain = "botsconstructor.com";
            string link = $"https://{domain}/SignUp/EmailCheckSuccess?guid={guid.ToString()}&email={model.Email}";
            return link;
        }


        [HttpGet]
        public IActionResult EmailCheckSuccess(Guid guid, [FromQuery(Name = "email")] string email)
        {
            var tmpAccount = context.TemporaryAccountWithUsernameAndPassword.
                FirstOrDefault(tmp => tmp.Email == email);

            if (tmpAccount != null)
            {
                Guid guidFromDb = tmpAccount.Guid;
                if (guidFromDb == guid)
                {
                   
                    string message = "Поздравляем, ваш email подтверждён";
                    return RedirectToAction("Success", "StaticMessage", new { message });
                }
                else
                {
                    string message = "В системе нет запроса на регистрацию акканута с таким идентификатором";
                    return RedirectToAction("Failure", "StaticMessage", new {message});
                }
            }
            else
            {
                string message = "В системе нет запроса на регистрацию такого акканута";
                return RedirectToAction("Failure", "StaticMessage", new {message});

            }
        }

       
        
    }
}