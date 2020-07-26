using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class EditSettingsModel
    {
        [StringLength(120, MinimumLength = 3, ErrorMessageResourceName = "EditSettingsUrlNameLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [RegularExpression("^[a-z0-9._]*$", ErrorMessageResourceName = "EditSettingsUrlNameAllowedCharacters", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string UrlName { get; set; }
    }
}