using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.ViewModels
{
    public class NavigationModel
    {
        public int BookId;
        public bool HasPrevious;
        public bool HasNext;
        public int PreviousStart;
        public int NextStart;
        public int CurrentStart;
    }
}