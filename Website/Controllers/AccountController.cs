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


/*Разбить эту хрень на пару хреней поменьше*/
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

        #region Регистрация аккаунта
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
                }else if (recordsWithTheSameAccountId.Count() == 1)
                {
                    recordsWithTheSameAccountId[0].GuidPasswordSentToEmail = guid;
                }
                else
                {
                    throw new Exception("В базе не должно быть больше одной записи для смены пароля");
                }

                string domain = HttpContext.Request.Host.Value;
                var link = $"https://{domain}/Account/PasswordResetOnlyNewPass?guid={guid.ToString()}&accountId={acc.Id}";

                
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

        [HttpPost]
        public IActionResult PasswordResetOnlyNewPass(ResetPasswordOnlyNewPassModel passModel, int accountId)
        {
            if (passModel!=null)
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


        [HttpGet]        
        public IActionResult EmailCheckSuccess(Guid guid, [FromQuery(Name = "accountId")] int accountId)
        {
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SuccessfulSend( string message)
        {
            ViewData["message"] = message;
            return View();
        }
    }
}

