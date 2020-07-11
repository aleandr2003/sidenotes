using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class EditBookModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Annotation { get; set; }

        public string AuthorEmail { get; set; }

        public string DonationMessage { get; set; }

        public string DonationForm { get; set; }

        public string CustomStyles { get; set; }

        public int PropertyStatus { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        [StringLength(100, ErrorMessageResourceName = "EditBookHashTagLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string HashTag { get; set; }
    }
}