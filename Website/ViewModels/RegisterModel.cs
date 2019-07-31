using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using website.Other.Attributes;

namespace website.ViewModels
{
    [EmailIsNotEqualToPasswordAttribute]
    public class RegisterModel
    {
        //[Required(ErrorMessage = "Не указан логин")]
        //[StringLength(20, MinimumLength = 3, ErrorMessage = "Длина логина должна быть от 3 до 20 символов")]
        //public string Login { get;  set; }

        [Required(ErrorMessage = "Не указан email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указано имя")]
        [StringLength(80, ErrorMessage = "Длина логина должна быть от 3 до 20 символов")]
        public string Name { get; set; }
      
        [Required(ErrorMessage = "Не указан пароль")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина пароля должна быть от 3 до 20 символов")]
        //[RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).*$", ErrorMessage = "Обязательно строчные и прописные латинские буквы, цифры")]
        [DataType(DataType.Password)]
        public string Password { get;  set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get;  set; }
    }
}
