using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLayer.Models;
using Website.Other;
using Website.Services;
using Website.ViewModels;

namespace Website.Controllers
{
    
    [Authorize]
    public class AccountManagementController : Controller
    {
        readonly ApplicationContext _contextDb;
        readonly StupidLogger _logger;

        public AccountManagementController(ApplicationContext applicationContext, StupidLogger logger)
        {
            _contextDb = applicationContext;
            this._logger = logger;
        }
       
        public IActionResult Index()
        {
            int accountId = (int)HttpContext.Items["accountId"];
            Account user = _contextDb.Accounts.Find(accountId);

            //Показ двух знаков после запятой
            decimal rounded = Math.Floor(user.Money * 100) / 100;

            ViewData["money"] = rounded;
            return View();
        }

        [HttpGet]
        
        public IActionResult ResetPassword()
        {
            int accountId = (int)HttpContext.Items["accountId"];
            Account user = _contextDb.Accounts.Find(accountId);

            if (user.Password == null)
            {
                //TODO Заглушка. Если пользователь логинится через телеграм, то пароля у него может и не быть
                //TODO Нужно редиректить на страницу с пояснением
                return RedirectToAction("Index", "AccountManagement");
            }

            return View();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordModel passModel)
        {

            int accountId = (int) HttpContext.Items["accountId"];
            Account account = _contextDb.Accounts.Find(accountId);

            if (account.Password != passModel.OldPassword)
            {
                ModelState.AddModelError("", "Текущий пароль введён неверно");
            }
            else if (passModel.NewPassword == null || passModel.ConfirmNewPassword == null)
            {
                ModelState.AddModelError("", "Заполните поля для нового пароля");
            }
            else if(passModel.NewPassword != passModel.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "Значения в полях для нового пароля не совпадают");
            }
            else
            {
                //Все данные введены правильно
                //Заменить пароль
                //TODO проверка пароля
                account.Password = passModel.NewPassword;
                _contextDb.SaveChanges();
                return RedirectToAction("Index", "AccountManagement");
            }


            return View(passModel);

        }


    }
}