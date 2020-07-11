using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SideNotes.ViewModels
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Пароль должен быть не короче 3 и не длиннее 32 символов")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Пароль должен быть не короче 3 и не длиннее 32 символов")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Пароль должен быть не короче 3 и не длиннее 32 символов")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Повторный ввод не совпадает с первым")]
        public string ConfirmNewPassword { get; set; }

    }
}