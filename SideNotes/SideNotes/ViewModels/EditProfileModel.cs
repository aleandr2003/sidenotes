using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class EditProfileModel
    {
        [Required(ErrorMessageResourceName = "EditProfileNameRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [StringLength(150, MinimumLength = 3, ErrorMessageResourceName = "EditProfileNameLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "EditProfileEmailRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [DataType(DataType.EmailAddress, ErrorMessageResourceName = "EditProfileEmailDataType", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string Email { get; set; }
    }
}