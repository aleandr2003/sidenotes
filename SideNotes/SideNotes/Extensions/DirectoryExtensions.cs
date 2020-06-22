using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SideNotes.Extensions
{
    public static class DirectoryExtensions
    {
        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}