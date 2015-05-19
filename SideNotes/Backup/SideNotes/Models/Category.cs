using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.JsonConverters;
using Newtonsoft.Json;

namespace SideNotes.Models
{
    public partial class Category
    {
        private Category parent;
        public Category Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        private List<Category> children;
        public List<Category> Children
        {
            get
            {
                if (children == null) children = new List<Category>();
                return children;
            }
        }
    }
}