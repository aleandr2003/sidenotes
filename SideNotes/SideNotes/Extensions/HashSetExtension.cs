using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Extensions
{
    public static class HashSetExtension
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach(T item in items)
            {
                set.Add(item);
            }
        }
    }
}