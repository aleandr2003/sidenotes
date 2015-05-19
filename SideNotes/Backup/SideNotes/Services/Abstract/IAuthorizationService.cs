using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Services.Abstract
{
    public interface IAuthorizationService
    {
        bool Authorize(Operation op, object obj);

        bool Authorize(Operation op);
    }
}