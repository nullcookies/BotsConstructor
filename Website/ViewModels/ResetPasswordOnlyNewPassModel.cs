using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace website.ViewModels
{
    public class ResetPasswordOnlyNewPassModel
    {


        [Required(ErrorMessage = "Не указан новый пароль")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина пароля должна быть от 3 до 20 символов")]
        //[RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).*$", ErrorMessage = "Обязательно строчные и прописные латинские буквы, цифры")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Новый пароль введен неверно")]
        public string ConfirmNewPassword { get; set; }

    }
}
