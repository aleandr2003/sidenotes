using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Net;
using System.IO;
using SideNotes.Extensions;
using System.Text;
using SideNotes.OAuth;
using System.Text.RegularExpressions;

namespace SideNotes.Services
{
    public class YandexSharer : Sharer
    {
        public YandexSharer(string AccessToken, User user)
            : base(AccessToken, user)
        {
        }

        //public string test(Paragraph paragraph, HeadComment comment)
        //{
        //    var urlHelper = UrlHelperExtensions.GetUrlHelper();
        //    var data = new StringBuilder(1024);
        //    data.Append("<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:y=\"http://api.yandex.ru/yaru/\" xmlns:thr=\"http://purl.org/syndication/thread/1.0\">");
        //    data.Append("<title>Заметки на полях</title>");
        //    data.Append("<y:access>public</y:access>");
        //    data.Append("<author>");
        //    data.Append("<y:id>urn:ya.ru:person/" + currentUser.YandexId + "</y:id>");
        //    data.Append("</author>");
        //    data.Append("<category term=\"link\" scheme=\"urn:ya.ru:posttypes\"/>");
        //    data.Append("<y:meta>");
        //    data.Append("<y:url>" + urlHelper.ActionAbsolute("CommentsByUser", "Book", new { BookId = paragraph.Book_Id, UserId = currentUser.Id }) + "</y:url>");
        //    data.Append("</y:meta>");
        //    data.Append("<content type=\"html\">" + createSnippet(paragraph, comment) + "</content>");
        //    data.Append("</entry>");
        //    return PostContent(data.ToString());
        //}

        private string createSnippet(Paragraph paragraph, HeadComment comment)
        {
            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            var data = new StringBuilder(1024);
            
            data.Append("<div style=\"background-color:#ccc; border-style:solid; border-color:#000; width:600px;\">");
            data.Append("<span style=\"font-style:italic;\">");
            data.Append(Regex.Replace(paragraph.Content, @"<(.|\n)*?>", string.Empty));
            data.Append("</span>");
            data.Append("<br/>");
            data.Append("<span>");
            data.Append(comment.Text);
            data.Append("</span>");
            data.Append("</div>");
            
            return data.ToString();
        }

        public override void ShareComment(Paragraph paragraph, HeadComment comment)
        {
            var urlHelper = UrlHelperExtensions.GetUrlHelper();
            var data = new StringBuilder(1024);
            data.Append("<entry xmlns=\"http://www.w3.org/2005/Atom\" xmlns:y=\"http://api.yandex.ru/yaru/\" xmlns:thr=\"http://purl.org/syndication/thread/1.0\">");
            data.Append("<title>Заметки на полях</title>");
            data.Append("<y:access>public</y:access>");
            data.Append("<author>");
            data.Append("<y:id>urn:ya.ru:person/" + currentUser.YandexId + "</y:id>");
            data.Append("</author>");
            data.Append("<category term=\"link\" scheme=\"urn:ya.ru:posttypes\"/>");
            data.Append("<y:meta>");
            data.Append("<y:url>" + urlHelper.ActionAbsolute("CommentsByUser", "Book", new { BookId = paragraph.Book_Id, UserId = currentUser.Id }) + "</y:url>");
            data.Append("</y:meta>");
            data.Append("<content type=\"html\">" + createSnippet(paragraph, comment) + "</content>");
            data.Append("</entry>");

            var client = new YandexClient(new YandexTokenManagerFactory().GetTokenManager());
            client.AccessToken = accessToken;
            client.MakePost(data.ToString(), currentUser.YandexId);
        }

        public override void ShareBook(Book book)
        {
        }
        
    }
}