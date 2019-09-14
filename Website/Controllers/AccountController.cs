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


namespace Website.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext _context;
        private EmailMessageSender _emailSender;

        public AccountController(ApplicationContext context, EmailMessageSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;

        }

        
        #region Логин
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        //Для теста
        //https://localhost:5001/Account/LoginWithTelegram?id=440090552&first_name=Ruslan&last_name=Starovoitov&username=shhar&photo_url=https%3A%2F%2Ft.me%2Fi%2Fuserpic%2F320%2Fshhar.jpg&auth_date=1564422079&hash=36b9be200d4866588e1b771f0587f45570fa3d834c9901ac92255105c2a94b7a
        //https://botsconstructor.com/Account/LoginWithTelegram?id=440090552&first_name=Ruslan&last_name=Starovoitov&username=shhar&photo_url=https%3A%2F%2Ft.me%2Fi%2Fuserpic%2F320%2Fshhar.jpg&auth_date=1564422079&hash=36b9be200d4866588e1b771f0587f45570fa3d834c9901ac92255105c2a94b7a

        //TODO Нужно протестировать
        [HttpGet]
        public IActionResult LoginWithTelegram(string id, string first_name, string last_name, string username, string photo_url, string auth_date, string hash)
        {

            string botToken = "913688656:AAGIJK2GQLFZTDGWjUX8jV5aPujLoHSiSus";

            List<string> myList = new List<string> {
                $"id={id}",
                $"auth_date={auth_date}" };

            //Эти поля могут быть пустыми
            if (first_name != null){
                myList.Add($"first_name={first_name}");
            }
            if (last_name != null){
                myList.Add($"last_name={last_name}");
            }
            if (username != null){
                myList.Add($"username={username}");
            }
            if (photo_url != null){
                myList.Add($"photo_url={photo_url}");
            }

            string[] myArr = myList.ToArray();

            Array.Sort(myArr);

            string data_check_string = string.Join("\n", myArr);

            Console.WriteLine(data_check_string);
            bool authorizationIsValid = false;


            using (SHA256 mySHA256 = SHA256.Create())
            {
                
                byte[] botTokenByteArr = Encoding.UTF8.GetBytes(botToken);
                byte[] secretKey = mySHA256.ComputeHash(botTokenByteArr);

                byte[] allUSerData = Encoding.UTF8.GetBytes(data_check_string);

                using (HMACSHA256 hmac = new HMACSHA256(secretKey))
                {
                    byte[] myValueByteArr = hmac.ComputeHash(allUSerData);

                    string calculatedHashString = BitConverter.ToString(myValueByteArr).Replace("-", string.Empty);
                    Console.WriteLine("Правильный ответ = " + hash);
                    Console.WriteLine("Мой ответ        = "+ calculatedHashString);

                    if(hash == calculatedHashString.ToLower())
                    {
                        authorizationIsValid = true;
                    }
                }
            }

            if (authorizationIsValid)
            {
                int.TryParse(id, out int telegramId);
                Account user = _context.Accounts.Where(_acc => _acc.TelegramId == telegramId).SingleOrDefault();
                if (user == null)
                {
                    user = new Account()
                    {
                        //TODO имя при логине через телеграм можно обновлять
                        Name = first_name + " " + last_name,
                        TelegramId = telegramId,
                        RoleTypeId = 1
                    };
                    _context.Accounts.Add(user);
                    _context.SaveChanges();
                }

                Authenticate(user);

                return RedirectToAction("Index", "Home");
                //Отправить сообщение о авторизации пользователю (приветствие)
            }

            ModelState.AddModelError("", "Ошибка авторизации");
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult PasswordRecovery()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            
            if (ModelState.IsValid)
            {
                Account account =  _context.Accounts
                    .FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);

                if (account != null)
                {
                    Authenticate(account); 

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль ");
            }
            return View(model);
        }
        #endregion

        #region Установка аутентификационных куки
        private void Authenticate(Account user)
        {
            string userRoleName = _context.AuthRoles.Where(role => role.Id == user.RoleTypeId).First().Name;
            // создаем один claim
            var claims = new List<Claim>
            {
                
                new Claim("userId", user.Id.ToString()),
                
                new Claim(ClaimsIdentity.DefaultRoleClaimType, userRoleName),
                
                new Claim("testType","testValue")
            };

            //создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            //установка аутентификационных куки
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        #endregion

        //TODO вынести в отдельный контроллер
        [HttpGet]        
        public IActionResult EmailCheckSuccess(Guid guid, [FromQuery(Name = "accountId")] int accountId)
        {
            //Ну кто так называет переменные?
            var ue   =  _context.UnconfirmedEmails.Where(_ue => _ue.AccountId == accountId).SingleOrDefault();
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
            return RedirectToAction("SuccessfulSend", new { message });
        }

     
    }
}

