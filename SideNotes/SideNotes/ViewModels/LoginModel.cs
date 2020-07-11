using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessageResourceName = "LoginEmailRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.EmailAddress, ErrorMessageResourceName = "LoginEmailDataType", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "LoginPasswordRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessageResourceName = "LoginPasswordLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Password { get; set; }

        public string Error { get; set; }
    }
}