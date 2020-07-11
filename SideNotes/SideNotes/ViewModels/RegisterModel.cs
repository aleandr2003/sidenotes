using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessageResourceName = "RegisterNameRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [StringLength(150, MinimumLength = 3, ErrorMessageResourceName = "RegisterNameLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "RegisterEmailRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.EmailAddress, ErrorMessageResourceName = "RegisterEmailDataType", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "RegisterPasswordRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessageResourceName = "RegisterPasswordLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Password { get; set; }
    }
}