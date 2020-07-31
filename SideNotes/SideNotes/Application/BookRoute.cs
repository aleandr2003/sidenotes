using SideNotes.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SideNotes.Application
{
    public class BookRoute : DomainRoute
    {
        private static ConcurrentDictionary<string, int> bookIndex;

        static BookRoute()
        {
            bookIndex = new ConcurrentDictionary<string, int>();
        }

        public static void Init(IBookRepository repository)
        {
            var books = repository.GetBooksWithDomainData();
            bookIndex.Clear();
            books.ForEach(book =>
            {
                bookIndex.TryAdd(CreateKey(book.UrlName, book.AnnotatorUrlName), book.Id);
                bookIndex.TryAdd(CreateKey(book.UrlName, String.Empty), book.Id);
            });
        }

        private static string CreateKey(string bookUrlName, string annotatorUrlName) => $"{bookUrlName}|{annotatorUrlName}";

        public BookRoute(string domain, string url, RouteValueDictionary defaults)
            : base(domain, url, defaults)
        {
        }

        public BookRoute(string domain, string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(domain, url, defaults, routeHandler)
        {
        }

        public BookRoute(string domain, string url, object defaults)
            : base(domain, url, defaults)
        {
        }

        public BookRoute(string domain, string url, object defaults, IRouteHandler routeHandler)
            : base(domain, url, defaults, routeHandler)
        {
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData data = base.GetRouteData(httpContext);
            if (data == null) return null;
            if (!data.Values.ContainsKey("booktitle"))
                return null;

            string booktitle = data.Values["booktitle"].ToString();
            string annotator = data.Values.ContainsKey("annotator") ? data.Values["annotator"].ToString() : String.Empty;
            string bookKey = CreateKey(booktitle, annotator);
            if(bookIndex.TryGetValue(bookKey, out int bookId))
            {
                data.Values["controller"] = "Book";
                data.Values["Id"] = bookId;
                return data;
            }
            return null;
        }
    }
}