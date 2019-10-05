using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.ViewModels
{
    public class LoginModel
    {
       
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Не указан логин")]
        public string Email { get;  set; }

        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get;  set; }
    }
}
