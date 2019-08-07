using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Services
{
    public interface IBookkeeper
    {
        decimal GetTodaysPrice(int botId);        
    }
}
