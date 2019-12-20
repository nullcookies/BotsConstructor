using System;
using System.Threading.Tasks;
using DataLayer;

namespace Website.Services
{
    public class AccountRegistrationService
    {
        private ApplicationContext dbContext;

        public AccountRegistrationService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task RegisterAccount(string name, EmailLoginInfo emailLoginInfo, TelegramLoginInfo telegramLoginInfo)
        {
            if (emailLoginInfo == null && telegramLoginInfo == null)
                throw new ArgumentException();
            
            var account = new Account
            {
                Name = name,
                RegistrationDate = DateTime.UtcNow,
                Money = 0,
                EmailLoginInfo =emailLoginInfo,
                TelegramLoginInfo = telegramLoginInfo
            };
            
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();
            
            var statusGroup = new OrderStatusGroup
            {
                Name = "Стандартный набор статусов",
                OwnerId = account.Id,
                OrderStatuses = new[]
                {
                    new OrderStatus {Name = "Просмотрено", Message = ""},
                    new OrderStatus {Name = "⏳В обработке", Message = "⏳Ваш заказ находится в обработке."},
                    new OrderStatus {Name = "🚚В пути", Message = "🚚Ваш заказ в пути."},
                    new OrderStatus {Name = "✅Принят", Message = "✅Ваш заказ был принят."},
                    new OrderStatus {Name = "❌Отменён", Message = "❌Ваш заказ был отменён."}
                }
            };
            await dbContext.OrderStatusGroups.AddAsync(statusGroup);
            await dbContext.SaveChangesAsync();
        }
    }
}