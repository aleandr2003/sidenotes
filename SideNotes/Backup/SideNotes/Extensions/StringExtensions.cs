using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace SideNotes.Extensions
{
    public static class StringExtensions
    {
        public static string MD5(this string source)
        {
            byte[] textBytes = System.Text.Encoding.Default.GetBytes(source);
            try
            {
                byte[] hash = (new MD5CryptoServiceProvider()).ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }
    }
}