using System;
using System.Threading.Tasks;
using DataLayer;

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
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (emailLoginInfo == null && telegramLoginInfo == null)
            {
                throw new ArgumentException();
            }
            
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
}