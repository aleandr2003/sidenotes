using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.OAuth;

namespace SideNotes.Services
{
    public class VkontakteSharer : Sharer
    {
        public VkontakteSharer(string AccessToken, User user)
            : base(AccessToken, user)
        {
        }

        //public string test(Paragraph paragraph, HeadComment comment)
        //{
        //    var client = new VkontakteClient(new VkontakteTokenManagerFactory().GetTokenManager());
        //    client.AccessToken = accessToken;
        //    client.UserId = int.Parse(currentUser.VkontakteId);
        //    return client.MakePost("Этот текст я запостил через Sidenotes"); 
        //}

        public override void ShareComment(Paragraph paragraph, HeadComment comment)
        {
            //var client = new VkontakteClient(new VkontakteTokenManagerFactory().GetTokenManager());
            //client.AccessToken = accessToken;
            //client.UserId = int.Parse(currentUser.VkontakteId);
            //client.MakePost("Этот текст я запостил через Sidenotes"); 
        }

        public override void ShareBook(Book book)
        {
        }
    }
}