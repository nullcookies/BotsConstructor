using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Services;
using Website.ViewModels;


namespace Website.Controllers.SignInUp
{
    public class PasswordResetController : Controller
    {

        private ApplicationContext _context;
        private EmailMessageSender _emailSender;

        public PasswordResetController(ApplicationContext context, EmailMessageSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;

        }

        [HttpGet]
        public IActionResult PasswordResetEnterEmail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordResetEnterEmail(string email)
        {
            if (!EmailMessageSender.EmailIsValid(email))
            {
                ModelState.AddModelError("", "Введённый email неверный");
                return View();
            }
            email = email.Trim();
            Account acc = _context.Accounts.Where(_acc => _acc.Email == email).SingleOrDefault();

            if (acc != null)
            {
                //Отправка сообщения
                Guid guid = Guid.NewGuid();

                AccountToResetPassword tmpRecordDb = new AccountToResetPassword() { AccountId = acc.Id, GuidPasswordSentToEmail = guid };

                //TODO Перезаписывать запрос на смену пароля если уже он уже есть
                List<AccountToResetPassword> recordsWithTheSameAccountId = _context.AccountsToResetPassword.Where(_tmpRecord => _tmpRecord.AccountId == acc.Id).ToList();
                if (recordsWithTheSameAccountId.Count() == 0)
                {
                    _context.AccountsToResetPassword.Add(tmpRecordDb);
                }
                else if (recordsWithTheSameAccountId.Count() == 1)
                {
                    recordsWithTheSameAccountId[0].GuidPasswordSentToEmail = guid;
                }
                else
                {
                    throw new Exception("В базе не должно быть больше одной записи для смены пароля");
                }

                string domain = HttpContext.Request.Host.Value;
                var link = $"https://{domain}/PasswordReset/PasswordResetOnlyNewPass?guid={guid.ToString()}&accountId={acc.Id}";


                bool SendIsOk = _emailSender.SendPasswordReset(email, acc.Name, link);


                if (!SendIsOk)
                {
                    //если email не отправился, то удалить из БД запись о возможности сброса пароля
                    _context.AccountsToResetPassword.Remove(tmpRecordDb);
                }
                _context.SaveChanges();

                string message = "На вашу почту отправлено письмо. Для того, чтобы сбросить пароль следуйте инструкциям в письме. ";
                return RedirectToAction("SuccessfulSend", new { message });

            }
            else
            {
                ModelState.AddModelError("", "Аккаунта с таким email не существует.");
            }

            return View();
        }


        [HttpGet]
        public IActionResult PasswordResetOnlyNewPass(Guid guid, int accountId)
        {
            var tmpRecord = _context.AccountsToResetPassword.Where(_acc => _acc.AccountId == accountId).SingleOrDefault();
            if (tmpRecord != null)
            {
                if (guid != null)
                {
                    if (guid == tmpRecord.GuidPasswordSentToEmail)
                    {
                        //запросить сброс пароля
                        ViewData["showPasswordEntryForm"] = true;
                        Account acc = _context.Accounts.Find(accountId);
                        ViewData["accountId"] = accountId;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Guid мне очень не нравится");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Guid  мне не нравится");
                }
            }
            else
            {
                ModelState.AddModelError("", "Этот аккаунт не запросил сброс пароля");
            }

            return View();
        }

        //TODO
        //Какого хера тут не проверяется guid?
        //Если человек запросил смену пароля, то пока он вводит новый и кто-то может его обогнать
        [HttpPost]
        public IActionResult PasswordResetOnlyNewPass(ResetPasswordOnlyNewPassModel passModel, int accountId)
        {
            if (passModel != null)
            {
                if (!string.IsNullOrEmpty(passModel.NewPassword))
                {
                    if (passModel.NewPassword == passModel.ConfirmNewPassword)
                    {
                        //проверка наличия запроса на замену пароля
                        AccountToResetPassword accToResDb = _context.AccountsToResetPassword.Where(accRes => accRes.AccountId == accountId).SingleOrDefault();
                        if (accToResDb != null)
                        {
                            Account acc = _context.Accounts.Find(accountId);

                            if (acc != null)
                            {
                                //удаление запроса на смену пароля
                                _context.AccountsToResetPassword.Remove(accToResDb);
                                //смена пароля
                                acc.Password = passModel.NewPassword;

                                _context.SaveChanges();

                                return RedirectToAction("Index", "Main");
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Критическая ошибка логики сервера. Не найден аккаунт для котогоро была запрошена процедура смены пароля. accountId={accountId} ");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", $"Критическая ошибка логики сервера. В бд не найдена запись (id, accid, guid) для сброса пароля. Если вы видите это сообщение,значит разработчик полный идиот. Напишите в тех. поддержку. Кто-то точно будет уволен. Хм, если это не я конечноже. accountId={accountId} ");
                        }
                        //заменить пароль в базе на этот
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Введённые пароли не совпадают.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", $"Пожалуйста, заполните оба поля.");
                }
            }
            else
            {
                ModelState.AddModelError("", $"Ошибка сервера. Неожиданный формат входящийх данных.");
            }


            return View();
        }

        //TODO вынести в отдельный контроллер
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SuccessfulSend(string message)
        {
            ViewData["message"] = message;
            return View();

        }
    }
}