using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;

namespace SideNotes.Models.Helpers
{
    public static class EntitiesHelper
    {
        [EdmFunction("SideNotesModel.Store", "IsMyFriend")]
        public static bool IsMyFriend(int HisId, int MyId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
    }
}