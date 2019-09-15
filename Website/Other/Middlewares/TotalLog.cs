﻿using DataLayer.Models;
using DataLayer.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Website.Other.Middlewares
{
    public class TotalLog
    {
        private StupidLogger _logger;
        

        public TotalLog(StupidLogger logger )
        {
            _logger = logger;
        }


        public void Log(HttpContext context)
        {
            
            string idStr = context.User?.FindFirst(x => x.Type == "userId")?.Value;
            int accountId = 0;
            int.TryParse(idStr, out accountId);

            string pathFrom = context.Request.Headers["Referer"].ToString();
            string pathCurrent = context.Request.Scheme+"://"+ context.Request.Host.Value + context.Request.Path;


            _logger.LogSpyRecord(pathCurrent, pathFrom, accountId);
        }
    }
}