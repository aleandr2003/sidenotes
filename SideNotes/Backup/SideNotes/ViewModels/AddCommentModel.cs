using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SideNotes.ViewModels
{
    public class AddCommentModel
    {
        [Required(ErrorMessage = "Не указан родительский комментарий")]
        public int parentCommentId { get; set; }

        [Required(ErrorMessage = "Не указан главный комментарий ветки")]
        public int headCommentId { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите текст комментария")]
        [StringLength(4000, ErrorMessage = "Комментарий должен быть не длиннее 4000 символов")]
        public string commentText { get; set; }
    }
}