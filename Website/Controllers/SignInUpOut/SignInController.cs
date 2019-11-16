using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Website.ViewModels;

namespace Website.Controllers.SignInUpOut
{
    public class SignInController : Controller
    {
        private readonly ApplicationContext _context;

        public SignInController(ApplicationContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MyBots", "MyBots");
            }
            else
            {
                return View();
            }
        }


        [HttpGet]
        public IActionResult LoginWithTelegram(string id,
            [FromQuery(Name = "first_name")] string firstName,
            [FromQuery(Name = "last_name")]  string lastName,
            [FromQuery(Name = "username")]   string username,
            [FromQuery(Name = "photo_url")]  string photoUrl,
            [FromQuery(Name = "auth_date")]  string authDate,
            [FromQuery(Name = "hash")]       string hash)
        {
            //@bots_constructor_bot
            string botToken = "913688656:AAH5l-ZAOmpI5MDqcq3ye1M1CF8ESF-Bms8";

            List<string> myList = new List<string>
            {
                $"id={id}",
                $"auth_date={authDate}"
            };

            //Эти поля могут быть пустыми
            if (firstName != null) myList.Add($"first_name={firstName}");
            if (lastName != null) myList.Add($"last_name={lastName}");
            if (username != null) myList.Add($"username={username}");
            if (photoUrl != null) myList.Add($"photo_url={photoUrl}");

            string[] myArr = myList.ToArray();

            Array.Sort(myArr);

            string dataCheckString = string.Join("\n", myArr);

            //Console.WriteLine(data_check_string);

            bool authorizationIsValid = false;

            using (SHA256 mySha256 = SHA256.Create())
            {
                byte[] botTokenByteArr = Encoding.UTF8.GetBytes(botToken);
                byte[] secretKey = mySha256.ComputeHash(botTokenByteArr);

                byte[] allUSerData = Encoding.UTF8.GetBytes(dataCheckString);

                using (HMACSHA256 hmac = new HMACSHA256(secretKey))
                {
                    byte[] myValueByteArr = hmac.ComputeHash(allUSerData);

                    string calculatedHashString = BitConverter.ToString(myValueByteArr).Replace("-", string.Empty);

                    //Console.WriteLine("Правильный ответ = " + hash);
                    //Console.WriteLine("Мой ответ        = "+ calculatedHashString);

                    if (hash == calculatedHashString.ToLower()) authorizationIsValid = true;
                }
            }

            if (authorizationIsValid)
            {
                int.TryParse(id, out int telegramId);

                Account user = _context.Accounts
                    .SingleOrDefault(_acc => _acc.TelegramId == telegramId);

                if (user == null)
                {
                    user = new Account
                    {
                        //TODO имя при логине через телеграм можно обновлять
                        Name = firstName + " " + lastName,
                        TelegramId = telegramId,
                        RoleTypeId = 1
                    };
                    _context.Accounts.Add(user);
                    _context.SaveChanges();
                }

                Authenticate(user);

                return RedirectToAction("MyBots", "MyBots");
                //TODO Отправить сообщение о авторизации пользователю (приветствие)
            }

            ModelState.AddModelError("", "Ошибка авторизации");
            return RedirectToAction("Login", "SignIn");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Account account = _context.Accounts
                    .FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);

                if (account != null)
                {
                    Authenticate(account);

                    return RedirectToAction("MyBots", "MyBots");
                }

                ModelState.AddModelError("", "Некорректные логин и(или) пароль ");
            }

            return View(model);
        }

        private void Authenticate(Account user)
        {

            string userRoleName = _context.AuthRoles.First(role => role.Id == user.RoleTypeId).Name;

            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
            };

            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}