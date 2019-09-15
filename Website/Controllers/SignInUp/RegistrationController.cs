﻿using System;
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
                            string link = $"https://{domain}/Registration/EmailCheckSuccess?guid={guid.ToString()}&accountId={account.Id}";

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


        //TODO вынести в отдельный контроллер
        [HttpGet]
        public IActionResult EmailCheckSuccess(Guid guid, [FromQuery(Name = "accountId")] int accountId)
        {
            //Ну кто так называет переменные?
            var ue = _context.UnconfirmedEmails.Where(_ue => _ue.AccountId == accountId).SingleOrDefault();
            if (ue != null)
            {
                Guid guidFromDb = ue.GuidPasswordSentToEmail;
                if (guidFromDb != null && guidFromDb == guid)
                {
                    Account acc = _context.Accounts.Find(accountId);
                    if (acc != null)
                    {
                        if (!string.IsNullOrEmpty(ue.Email))
                        {
                            //Присвоить почту аккаунту
                            acc.Email = ue.Email;
                            //убрать запись из таблицы неподтверждённых email
                            _context.UnconfirmedEmails.Remove(ue);
                            _context.SaveChanges();

                        }
                        else
                        {
                            ModelState.AddModelError("", "Ошибка логики сервера. В базе данных не найден email, который нужно подтвердить.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ошибка логики сервера. В базе данных не найден аккаунт, к которому нужно привязать email.");
                    }
                }
                else
                {
                    //Вы пытаетесь мне навредить. Мне это очень не нравится.
                    //ModelState.AddModelError("", "👆 🔄 🤕 👤. 👤 🚫 💖 👉 📶 💗.");
                    ModelState.AddModelError("", $"Мне не нравится guid accountId={accountId},guid={guid}");

                }

            }
            else
            {
                //Вы пытаетесь мне навредить. Мне это очень не нравится.
                ModelState.AddModelError("", $"В базе нет запроса на подтверждение accountId={accountId},guid={guid}");
            }



            string message = "Поздравляем, ваш email подтверждён";
            return RedirectToAction("Success", "StaticMessage", new { message });
        }

    }
}