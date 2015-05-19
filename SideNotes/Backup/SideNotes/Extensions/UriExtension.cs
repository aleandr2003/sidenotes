using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Extensions
{
    public static class UriExtension
    {
        public static Uri AddParameter(this Uri source, string name, string value)
        {
            if (String.IsNullOrEmpty(source.Query))
            {
                return new Uri(source + "?" + name + "=" + value);
            }
            else
            {
                return new Uri(source + "&" + name + "=" + value);
            }
        }

        public static Dictionary<string, string> GetParameters(this Uri source)
        {
            string paramString = String.IsNullOrEmpty(source.Query) ? String.Empty : source.Query.Substring(1);
            return paramString.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new { name = s.Substring(0, s.IndexOf("=")), value = s.Substring(s.IndexOf("=") + 1) })
                .ToDictionary(s => s.name, s => s.value);
        }

        public static Uri SetParameter(this Uri source, string name, string value)
        {
            var parameters = source.GetParameters();
            if (!parameters.Keys.Contains(name))
            {
                return source.AddParameter(name, value);
            }
            else
            {
                var url = source.AbsoluteUri.Substring(0, source.AbsoluteUri.IndexOf("?"));
                parameters[name] = value;
                return new Uri(url + "?" + String.Join("&", parameters.Select(p => p.Key + "=" + p.Value)));
            }
        }
    }
}