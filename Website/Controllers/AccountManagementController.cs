using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLayer.Models;
using Website.Other;
using Website.Services;
using Website.ViewModels;
using DataLayer.Services;
 
/*какого два класса для аккаунта*/
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
        //[HttpGet]
        //public IActionResult GiveMeMoney()
        //{
        //    int accId = 0;
        //    try{
        //        accId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
        //    }catch{
        //        return Forbid();
        //    }

        //    Account user = _contextDb.Accounts.Find(accId);

        //    user.Money += (decimal) 546.1518434168;
        //    _contextDb.SaveChanges();

        //    return Ok();

        //}




        //[HttpGet]
        //public IActionResult TakeItAway()
        //{

        //    int accId = 0;
        //    try{
        //        accId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
        //    }catch{
        //        return Forbid();
        //    }

        //    Account user = _contextDb.Accounts.Find(accId);

        //    if (user.Money > 0)
        //    {
        //        user.Money -= (decimal)660.19156871163;
        //        _contextDb.SaveChanges();
        //    }

        //    return Ok();



        //}

        public IActionResult Index()
        {
            int accId = 0;
            try{
                accId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Login", "SignIn");
            }

            Account user = _contextDb.Accounts.Find(accId);
            decimal money = user.Money;

            //Показ двух знаков после запятой
            decimal rounded = Math.Floor(money * 100) / 100;
           

            ViewData["money"] = rounded;
            return View();
        }

        [HttpGet]
        
        public IActionResult ResetPassword()
        {
            int accId = 0;
            try{
                accId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Login", "SignIn");
            }

            Account user = _contextDb.Accounts.Find(accId);
            if (user.Password == null)
            {
                //TODO Заглушка. Если пользователь логинится через телеграм, то пароля у него может и не быть
                return RedirectToAction("Index", "AccountManagement");
            }

            return View();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordModel passModel)
        {

            int accId = 0;
            try{
                accId = Stub.GetAccountIdFromCookies(HttpContext) ?? throw new Exception("Аккаунт с таким id  не найден.");
            }catch{
                return RedirectToAction("Login", "SignIn");
            }


            Account acc = _contextDb.Accounts.Find(accId);

            if (acc == null)
            {
                //Критическая ошибка безопасности
                //Почему в БД нет такого аккаунта?
                //Ведь пользователь авторизован
                _logger.Log(
                    LogLevelMyDich.CRITICAL_SECURITY_ERROR, 
                    Source.WEBSITE, 
                    "Почему в БД нет такого аккаунта? Ведь пользователь авторизован", 
                    accountId:accId);

                return RedirectToAction("Logout", "SignOut");
            }

            if (acc.Password != passModel.OldPassword)
            {
                ModelState.AddModelError("", "Текущий пароль введён неверно");
            }
            else if (passModel.NewPassword == null || passModel.ConfirmNewPassword == null)
            {
                ModelState.AddModelError("", "Заполните поля для нового пароля");
            }
            else if(passModel.NewPassword!=passModel.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "Поля для нового пароля не совпадают");
            }
            else
            {
                //Все данные введены правильно
                //Заменить пароль
                acc.Password = passModel.NewPassword;
                _contextDb.SaveChanges();
                return RedirectToAction("Index", "AccountManagement");
            }

            

            return View(passModel);

        }


    }
}