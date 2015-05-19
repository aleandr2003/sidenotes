using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace SideNotes.Services.Abstract
{
    public interface IEBookParser:IDisposable
    {
        void Parse(Stream input);
    }
}