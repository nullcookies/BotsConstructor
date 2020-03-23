using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using Website.Other;
using Website.Services;
using Website.ViewModels;

namespace Website.Controllers.SignInUpOut
{
    public class PasswordResetController : Controller
    {

        private readonly SimpleLogger logger;
        private readonly ApplicationContext context;
        private readonly EmailMessageSender emailSender;
        private DomainNameService domainNameService;
        public PasswordResetController(ApplicationContext context, 
            EmailMessageSender emailSender,
            SimpleLogger logger, DomainNameService domainNameService)
        {
            this.context = context;
            this.emailSender = emailSender;
            this.logger = logger;
            this.domainNameService = domainNameService;
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
            
            Account account = context.EmailLoginInfo
                .Include(info=>info.Account)
                .SingleOrDefault(_acc => _acc.Email == email)?.Account;

            if (account != null)
            {
                //Отправка сообщения

                Guid guid = Guid.NewGuid();

                AccountToResetPassword tmpRecordDb = new AccountToResetPassword() { AccountId = account.Id, GuidPasswordSentToEmail = guid };

                //TODO Перезаписывать запрос на смену пароля если уже он уже есть
                List<AccountToResetPassword> recordsWithTheSameAccountId = context.AccountsToResetPassword
                    .Where(_tmpRecord => _tmpRecord.AccountId == account.Id)
                    .ToList();

                if (!recordsWithTheSameAccountId.Any())
                {
                    context.AccountsToResetPassword.Add(tmpRecordDb);
                }
                else if (recordsWithTheSameAccountId.Count() == 1)
                {
                    recordsWithTheSameAccountId[0].GuidPasswordSentToEmail = guid;
                }
                else
                {
                    throw new Exception("В базе не должно быть больше одной записи для смены пароля");
                }

                string domain = domainNameService.GetDomainName();
                var link = $"https://{domain}/PasswordReset/PasswordResetOnlyNewPass?guid={guid.ToString()}&accountId={account.Id}";


                bool sendIsOk = emailSender.SendPasswordReset(email, account.Name, link);


                if (!sendIsOk)
                {
                    //если email не отправился, то удалить из БД запись о возможности сброса пароля
                    context.AccountsToResetPassword.Remove(tmpRecordDb);
                }
                context.SaveChanges();

                string message = "На вашу почту отправлено письмо. Для того, чтобы сбросить пароль следуйте инструкциям в письме. ";
                return RedirectToAction("Success","StaticMessage", new { message });

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
            var tmpRecord = context.AccountsToResetPassword
                .SingleOrDefault(_acc => _acc.AccountId == accountId);

            if (tmpRecord != null)
            {
                if (guid == tmpRecord.GuidPasswordSentToEmail)
                {
                    //запросить сброс пароля
                    ViewData["showPasswordEntryForm"] = true;
                    ViewData["accountId"] = accountId;
                    ViewData["guid"] = guid;
                }
                else
                {
                    ModelState.AddModelError("", "Guid мне очень не нравится");
                }
            }
            else
            {
                ModelState.AddModelError("", "Этот аккаунт не запросил сброс пароля");
            }

            return View();
        }

        [HttpPost]
        public IActionResult PasswordResetOnlyNewPass(ResetPasswordOnlyNewPassModel passModel, [FromQuery(Name = "targetAccountId")] int targetAccountId, Guid guid)
        {
            //Проверка guid-a
            var accountToResetPass = context.AccountsToResetPassword
               .SingleOrDefault(_acc => _acc.AccountId == targetAccountId);


            if (accountToResetPass == null || guid != accountToResetPass.GuidPasswordSentToEmail)
            {
                int accountId;
                try{
                    accountId = HttpClientWrapper.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
                }catch{
                    return StatusCode(403);
                }

                logger.Log(LogLevel.UNAUTHORIZED_ACCESS_ATTEMPT,
                    Source.PASSWORD_RESET,
                    $"Аккаунт {targetAccountId} запросил смену пароля. В это время пришёл post запрос " +
                    $"с новым паролем, но guid был неверным. guid={guid}, accountId из cookie = {accountId}");

                return Forbid();
            }


            //guid проверен. Всё ок. Можно менять пароль.

            if (passModel != null)
            {
                if (!string.IsNullOrEmpty(passModel.NewPassword))
                {
                    if (passModel.NewPassword == passModel.ConfirmNewPassword)
                    {
                        {
                            var emailLoginInfo = context.EmailLoginInfo
                                .SingleOrDefault(info => info.AccountId==targetAccountId);

                            if (emailLoginInfo != null)
                            {
                                //удаление запроса на смену пароля
                                context.AccountsToResetPassword.Remove(accountToResetPass);
                                //смена пароля
                                emailLoginInfo.Password = passModel.NewPassword;

                                context.SaveChanges();

                                return RedirectToAction("Index", "Main");
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Критическая ошибка логики сервера. Не найден аккаунт для котогоро была запрошена процедура смены пароля. accountId={targetAccountId} ");
                            }
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

       
    }
}