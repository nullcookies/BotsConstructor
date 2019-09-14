using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Website.Services;
using Website.ViewModels;

namespace Website.Controllers.SignInUp
{
    public class RegistrationController : Controller
    {

        private ApplicationContext _context;
        private EmailMessageSender _emailSender;

        public RegistrationController(ApplicationContext context, 
            EmailMessageSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;


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


                bool thereIsNoSuchEmailYet = _context.Accounts.FirstOrDefault(a => a.Email == model.Email) == null;

                if (thereIsNoSuchEmailYet)
                {

                    //TODO это какая-то дичь
                    //Выбрать последний id
                    int? oldId = _context.Accounts.LastOrDefault()?.Id;
                    int nextId = oldId.GetValueOrDefault() + 100;


                    Account account = new Account
                    {
                        //Разобраться как использоватьэту хрень без id
                        Id = nextId,

                        //Сначала нужно подтвердить email
                        //Email = model.Email,
                        Name = model.Name,
                        Password = model.Password,
                        RoleTypeId = 1
                    };

                    _context.Accounts.Add(account);

                    //Отправка сообщения на указанный email, чтобы удостовериться, что он принадлежит этому пользователю
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        if (EmailMessageSender.EmailIsValid(model.Email))
                        {


                            Guid guid = Guid.NewGuid();
                            string domain = HttpContext.Request.Host.Value;
                            string link = $"https://{domain}/Account/EmailCheckSuccess?guid={guid.ToString()}&accountId={account.Id}";

                            var unconfirmedEmail = new UnconfirmedEmail() { AccountId = account.Id, Email = model.Email, GuidPasswordSentToEmail = guid };

                            _context.UnconfirmedEmails.Add(unconfirmedEmail);

                            //Отправка сообщения
                            bool SendIsOk = _emailSender.SendEmailCheck(model.Email, model.Name, link);

                            if (!SendIsOk)
                            {
                                //если email не отправился, то удалить из БД запись о нём
                                _context.UnconfirmedEmails.Remove(unconfirmedEmail);
                            }


                        }

                    }
                    _context.SaveChanges();

                    /*await*/
                    //Authenticate(account);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Аккаунт с таким email уже существует");

                }


            }
            return View(model);
        }
    }
}