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
        [Required(ErrorMessageResourceName = "ChangePasswordOldPasswordRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessageResourceName = "ChangePasswordPasswordLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "ChangePasswordNewPasswordRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessageResourceName = "ChangePasswordPasswordLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName = "ChangePasswordConfirmNewPasswordRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 3, ErrorMessageResourceName = "ChangePasswordPasswordLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceName = "ChangePasswordConfirmNewPasswordCompare", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string ConfirmNewPassword { get; set; }

    }
}