using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;

namespace Website.Controllers.SignInUpOut
{
    public static class StatusGroupsAdder
    {
        public static void AddDefaultStatusGroup(this Account account)
        {
            //TODO: менять сообщения и названия в зависимости от языка владельца бота
            var statusGroup = new OrderStatusGroup()
            {
                Name = "Стандартный набор статусов",
                OrderStatuses = new List<OrderStatus>()
                {
                    new OrderStatus() {Name = "👀Просмотрено", Message = ""},
                    new OrderStatus() {Name = "⏳В обработке", Message = "⏳Ваш заказ находится в обработке."},
                    new OrderStatus() {Name = "🚚В пути", Message = "🚚Ваш заказ в пути."},
                    new OrderStatus() {Name = "✅Принят", Message = "✅Ваш заказ был принят."},
                    new OrderStatus() {Name = "❌Отменён", Message = "❌Ваш заказ был отменён."}
                }
            };

            account.OrderStatusGroups.Add(statusGroup);
        }
    }
}
