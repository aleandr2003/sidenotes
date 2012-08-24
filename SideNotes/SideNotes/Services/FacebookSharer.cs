using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Net;
using System.IO;
using SideNotes.Extensions;
using SideNotes.OAuth;
using System.Text.RegularExpressions;

namespace SideNotes.Services
{
    public class FacebookSharer : Sharer
    {
        private const int commentLength = 200;
        private const int bookTextLength = 300;

        public FacebookSharer(string AccessToken, User user)
            : base(AccessToken, user)
        {
        }

        //public string test(Paragraph paragraph, HeadComment comment)
        //{
           
        //}

        public override void ShareComment(Paragraph paragraph, HeadComment comment)
        {
            var message = comment.Text;
            if (message.Length > commentLength) message = message.Substring(0, commentLength) + "...";

            var bookText = String.Empty;

            if (!(paragraph is ImageParagraph) && !(paragraph is EmptyLine))
            {
                bookText = Regex.Replace(paragraph.Content, @"<(.|\n)*?>", string.Empty);
            }
            if (bookText.Length > bookTextLength) bookText = bookText.Substring(0, bookTextLength) + "...";

            string linkName = null;
            string avatarUrl = null;
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.Include("Avatar.Small").FirstOrDefault(b => b.Id == paragraph.Book_Id);
                if (book == null) throw new ArgumentException("книга не найдена");
                linkName = book.Title + " (" + book.Author + ")";
                if (book.Avatar != null) avatarUrl = VirtualPathUtility.ToAbsolute(book.Avatar.Small.Url);
                else avatarUrl = VirtualPathUtility.ToAbsolute("~/Content/img/sidenotes_logo.jpg");
            }

            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            avatarUrl = urlHelper.Absolute(avatarUrl).ToString();
            string linkTitle = urlHelper.ActionAbsolute("View", "Book", new { Id = paragraph.Book_Id }).ToString();
            string linkUrl = urlHelper.ActionAbsolute("View", "Book", new { Id = paragraph.Book_Id, skip = paragraph.OrderNumber - 1, expanded ="on" }).ToString();
            var data = String.Format("message={0}&picture={1}&link={2}&name={3}&caption={4}&description={5}",
                message, avatarUrl, linkUrl, linkName, linkTitle, bookText);

            var client = new FacebookClient(new FacebookTokenManagerFactory().GetTokenManager());
            client.AccessToken = accessToken;
            client.MakePost(data);
        }

        public override void ShareBook(Book book)
        {
            var message = String.Format("Читаю '{0}' на sidenotes.ru", book.Title);
            if (message.Length > commentLength) message = message.Substring(0, commentLength) + "...";

            var bookText = book.Annotation;
            if (bookText.Length > bookTextLength) bookText = bookText.Substring(0, bookTextLength) + "...";

            string linkName = null;
            string avatarUrl = null;
            using (var context = new SideNotesEntities())
            {
                var avatar = context.Avatars.Include("Small").FirstOrDefault(a => a.Id == book.Avatar_Id);
                linkName = book.Title + " (" + book.Author + ")";
                if (avatar != null) avatarUrl = VirtualPathUtility.ToAbsolute(avatar.Small.Url);
                else avatarUrl = VirtualPathUtility.ToAbsolute("~/Content/img/sidenotes_logo.jpg");
            }

            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            avatarUrl = urlHelper.Absolute(avatarUrl).ToString();
            string linkTitle = urlHelper.ActionAbsolute("Annotation", "Book", new { Id = book.Id }).ToString();
            string linkUrl = urlHelper.ActionAbsolute("Annotation", "Book", new { Id = book.Id}).ToString();
            var data = String.Format("message={0}&picture={1}&link={2}&name={3}&caption={4}&description={5}",
                message, avatarUrl, linkUrl, linkName, linkTitle, bookText);

            var client = new FacebookClient(new FacebookTokenManagerFactory().GetTokenManager());
            client.AccessToken = accessToken;
            client.MakePost(data);
        }
    }
}