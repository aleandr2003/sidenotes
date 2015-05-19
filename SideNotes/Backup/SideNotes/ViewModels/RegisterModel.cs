using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите имя пользователя")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть не короче 3 и не длиннее 150 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите e-mail")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введен некорректный e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Пароль должен быть не короче 3 и не длиннее 32 символов")]
        public string Password { get; set; }
    }
}