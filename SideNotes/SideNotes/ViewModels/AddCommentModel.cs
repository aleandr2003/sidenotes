using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class AddCommentModel
    {
        [Required(ErrorMessageResourceName = "AddCommentParentCommentRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public int parentCommentId { get; set; }

        [Required(ErrorMessageResourceName = "AddCommentHeadCommentRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public int headCommentId { get; set; }

        [Required(ErrorMessageResourceName = "AddCommentTextRequired", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        [StringLength(4000, ErrorMessageResourceName = "AddCommentTextLength", ErrorMessageResourceType = typeof(global::Resources.ValidationResources))]
        public string commentText { get; set; }
    }
}