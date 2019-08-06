using DataLayer.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Other.Middlewares
{
    public class TotalLog
    {
        private StupidLogger _logger;
        

        public TotalLog(StupidLogger logger)
        {
            _logger = logger;
        }


        public void Log(HttpContext context)
        {
            
            //Определить accountId
            string idStr = context.User?.FindFirst(x => x.Type == "userId")?.Value;
            int accountId = int.MinValue;
            int.TryParse(idStr, out accountId);
            

            _logger.Log(DataLayer.Models.LogLevelMyDich.INFO, Source.WEBSITE, "тест логирования 222", accountId:accountId);

            

        }
    }
}
