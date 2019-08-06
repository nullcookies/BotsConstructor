using DataLayer.Models;
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
            
            string idStr = context.User?.FindFirst(x => x.Type == "userId")?.Value;
            int accountId = 0;
            int.TryParse(idStr, out accountId);

            string path = context.Request.Path;

            _logger.Log(
                LogLevelMyDich.SPYING, 
                Source.WEBSITE, 
                $"path={path}", 
                accountId: accountId);
        }
    }
}
