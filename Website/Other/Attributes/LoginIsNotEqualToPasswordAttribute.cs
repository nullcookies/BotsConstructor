using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using website.Models;
using website.ViewModels;

namespace website.Other.Attributes
{
    /// <summary>
    /// Самописный атрибут. Проверяет, что логин не равен паролю в форме регистрации
    /// </summary>
    public class EmailIsNotEqualToPasswordAttributeAttribute : ValidationAttribute
    {
        public EmailIsNotEqualToPasswordAttributeAttribute()
        {
            ErrorMessage = "Имя и пароль не должны совпадать!";
        }
        public override bool IsValid(object value)
        {
            RegisterModel reg = value as RegisterModel;

            if (reg.Email == reg.Password)
            {
                return false;
            }
            return true;
        }

    }
}
