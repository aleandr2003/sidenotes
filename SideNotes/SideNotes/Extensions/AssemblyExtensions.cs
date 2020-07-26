using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace SideNotes.Extensions
{
    public static class AssemblyExtensions
    {
        public static List<string> GetControllerNames(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type)).Select(type => type.Name).ToList();
        }
    }
}