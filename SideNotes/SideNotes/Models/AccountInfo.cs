using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;

namespace SideNotes.Models
{
    public class AccountInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string ProfileUrl { get; set; }
        public AccountSource AccountSource { get; set; }

        public virtual Stream GetAvatarInputStream()
        {
            try
            {
                if (String.IsNullOrEmpty(Photo)) return null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Photo);
                WebResponse response = request.GetResponse();

                return response.GetResponseStream();
            }
            catch (WebException ex)
            {
                return null;
            }
        }
    }
}