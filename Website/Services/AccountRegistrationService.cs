using System;
using System.Threading.Tasks;
using DataLayer;
using JetBrains.Annotations;

namespace Website.Services
{
    public class AccountRegistrationService
    {
        private readonly ApplicationContext dbContext;

        public AccountRegistrationService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Account> RegisterAccountAsync(string name, EmailLoginInfo emailLoginInfo )
        {
            return await RegisterAccountAsync(name, emailLoginInfo,null);
        }
        public async Task<Account> RegisterAccountAsync(string name, TelegramLoginInfo telegramLoginInfo )
        {
            return await RegisterAccountAsync(name, null,telegramLoginInfo);
        }
        
        private async Task<Account> RegisterAccountAsync(string name, EmailLoginInfo emailLoginInfo, TelegramLoginInfo telegramLoginInfo)
        {
            //TODO добавить сервисы для проверки входных данных
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            LoginInfoCheckService.CheckEmailLoginInfo(emailLoginInfo);

            var account = new Account
            {
                Name = name,
                RegistrationDate = DateTime.UtcNow,
                Money = 49.99M,
                EmailLoginInfo = emailLoginInfo,
                TelegramLoginInfo = telegramLoginInfo,
                OrderStatusGroups = {
                    new OrderStatusGroup
                    {
                        Name = "Стандартный набор статусов",
                        OrderStatuses = new[]
                        {
                            new OrderStatus {Name = "Просмотрено", Message = ""},
                            new OrderStatus {Name = "⏳В обработке", Message = "⏳Ваш заказ находится в обработке."},
                            new OrderStatus {Name = "🚚В пути", Message = "🚚Ваш заказ в пути."},
                            new OrderStatus {Name = "✅Принят", Message = "✅Ваш заказ был принят."},
                            new OrderStatus {Name = "❌Отменён", Message = "❌Ваш заказ был отменён."}
                        }
                    }
                }
            };
            
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            return account;
        }

       
    }

    public static class LoginInfoCheckService
    {
        public static void CheckEmailLoginInfo([NotNull] EmailLoginInfo emailLoginInfo)
        {
            if (emailLoginInfo == null)
            {
                throw new ArgumentNullException(nameof(emailLoginInfo));
            }

            if (emailLoginInfo.Email == null)
            {
                throw new NullReferenceException(nameof(emailLoginInfo.Email));
            }
            
            if (emailLoginInfo.Password == null)
            {
                throw new NullReferenceException(nameof(emailLoginInfo.Password));
            }

            if (!PasswordIsOk(emailLoginInfo.Password))
            {
                throw new Exception("Bad password");
            }
        }

        private static bool PasswordIsOk(string pass)
        {
            if (pass.Length < 6)
            {
                return false;
            }
            
            if (pass.Contains(" "))
            {
                return false;
            }
            
            if (pass.Length > 50)
            {
                return false;
            }

            return true;
        }
    }
}