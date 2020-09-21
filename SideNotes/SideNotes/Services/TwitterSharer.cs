using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.OAuth;
using SideNotes.Extensions;

namespace SideNotes.Services
{
    public class TwitterSharer : Sharer
    {
        private const int TweetLength = 140;
        private const int LinkLength = 20;
        public TwitterSharer(string AccessToken, User user)
            : base(AccessToken, user)
        {
        }

        //public string test(Paragraph paragraph, HeadComment comment)
        //{
        //    var urlHelper = UrlHelperExtensions.GetUrlHelper();
        //    string link = urlHelper.Action("CommentsByUser", "Book", new { Id = paragraph.Book_Id, UserId = currentUser.Id }, true).ToString();

        //    string hashTag = null;
        //    using (var context = new SideNotesEntities())
        //    {
        //        hashTag = context.Books.Where(b => b.Id == paragraph.Id).Select(b => b.HashTag).FirstOrDefault();
        //        if (!String.IsNullOrEmpty(hashTag) && !hashTag.StartsWith("#")) hashTag = "#" + hashTag;
        //        if (hashTag == null) hashTag = String.Empty;
        //    }
        //    string commentText = comment.Text;
        //    if (commentText.Length > TweetLength - LinkLength - hashTag.Length - 10)
        //    {
        //        int commentLength = TweetLength - LinkLength - hashTag.Length - 10;
        //        commentText = commentText.Substring(0, commentLength) + "... ";
        //    }
        //    commentText += " " + hashTag + " " + link;

        //    var client = new TwitterClient(new TwitterTokenManagerFactory().GetTokenManager());
        //    client.AccessToken = accessToken;
        //    return client.MakeTweet(commentText);
        //}

        public override void ShareComment(Paragraph paragraph, HeadComment comment)
        {
            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            string link = urlHelper.Action("View", "Book", new { Id = paragraph.Book_Id, skip = paragraph.OrderNumber - 1, expanded = "on" }, true).ToString();

            string hashTag = null;
            using (var context = new SideNotesEntities())
            {
                hashTag = context.Books.Where(b => b.Id == paragraph.Book_Id).Select(b => b.HashTag).FirstOrDefault();
                if (!String.IsNullOrEmpty(hashTag) && !hashTag.StartsWith("#")) hashTag = "#" + hashTag;
                if (hashTag == null) hashTag = String.Empty;
            }
            string commentText = comment.Text;
            if (commentText.Length > TweetLength - LinkLength - hashTag.Length - 10)
            {
                int commentLength = TweetLength - LinkLength - hashTag.Length - 10;
                commentText = commentText.Substring(0, commentLength) + "... ";
            }
            commentText += " " + hashTag + " " + link;

            var client = new TwitterClient(new TwitterTokenManagerFactory().GetTokenManager());
            client.AccessToken = accessToken;
            client.MakeTweet(commentText);
        }

        public override void ShareBook(Book book)
        {
            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            string link = urlHelper.Action("Annotation", "Book", new { Id = book.Id}, true).ToString();

            //string hashTag = book.HashTag;
            //if (!String.IsNullOrEmpty(hashTag) && !hashTag.StartsWith("#")) hashTag = "#" + hashTag;
            //if (hashTag == null) hashTag = String.Empty;
            
            string commentText = String.Format(Resources.Misc.TwitterSharerIReadTheBook, book.Title);
            if (commentText.Length > TweetLength - LinkLength - 10)
            {
                int commentLength = TweetLength - LinkLength - 10;
                commentText = commentText.Substring(0, commentLength) + "... ";
            }
            commentText += " " + link;

            var client = new TwitterClient(new TwitterTokenManagerFactory().GetTokenManager());
            client.AccessToken = accessToken;
            client.MakeTweet(commentText);
        }
        
    }
}