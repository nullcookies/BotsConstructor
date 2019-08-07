using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Services.Bookkeeper
{
    public class BotForSalesBookkeeper : IBookkeeper
    {
        ApplicationContext _contextDb;
        public BotForSalesBookkeeper(ApplicationContext context)
        {
            _contextDb = context;
        }

        public decimal GetTodaysPrice(int botId)
        {
            throw new NotImplementedException();
        }
    }
}
