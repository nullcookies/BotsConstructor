using DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;
using Website.ViewModels;

namespace Website.Controllers.Private_office.AccountManagement
{
    
    [Authorize]
    public class AccountManagementController : Controller
    {
        readonly ApplicationContext contextDb;
        readonly SimpleLogger logger;

        public AccountManagementController(ApplicationContext applicationContext, SimpleLogger logger)
        {
            contextDb = applicationContext;
            this.logger = logger;
        }
       
        public IActionResult Index()
        {
            int accountId = (int)HttpContext.Items["accountId"];
            Account user = contextDb.Accounts.Find(accountId);

            //Показ двух знаков после запятой
            ViewData["money"] = $"{user.Money:0.00}";
            return View();
        }

        [HttpGet]
        
        public IActionResult ResetPassword()
        {
            int accountId = (int)HttpContext.Items["accountId"];
            Account user = contextDb.Accounts.Find(accountId);

            if (user.EmailLoginInfo.Password == null)
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
            Account account = contextDb.Accounts.Find(accountId);

            if (account.EmailLoginInfo.Password != passModel.OldPassword)
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
                account.EmailLoginInfo.Password = passModel.NewPassword;
                contextDb.SaveChanges();
                return RedirectToAction("Index", "AccountManagement");
            }


            return View(passModel);

        }


    }
}