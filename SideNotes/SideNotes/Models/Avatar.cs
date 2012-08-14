using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public partial class Avatar
    {
        public Photo[] AllPhotos
        {
            get
            {
                return (new Photo[] { Original, Large, Medium, Small, Tiny })
                    .Where(p => p != null).ToArray();
            }
        }
    }
}