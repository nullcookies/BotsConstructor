using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Website.Controllers;
using Website.Other.Attributes;

namespace Website.ViewModels
{
    
    public class TokenChange
    {
        public string Token { get;  set; }
        public BotType BotType { get;  set; }
    }
}
