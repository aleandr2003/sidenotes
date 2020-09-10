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
        private static ConcurrentDictionary<int, Tuple<string, string>> bookDataIndex;

        static BookRoute()
        {
            bookIndex = new ConcurrentDictionary<string, int>();
            bookDataIndex = new ConcurrentDictionary<int, Tuple<string, string>>();
        }

        public static void Init(IBookRepository repository)
        {
            var books = repository.GetBooksWithDomainData();
            bookIndex.Clear();
            books.ForEach(book =>
            {
                bookIndex.TryAdd(CreateKey(book.UrlName, book.AnnotatorUrlName), book.Id);
                bookIndex.TryAdd(CreateKey(book.UrlName, String.Empty), book.Id);
                bookDataIndex.TryAdd(book.Id, new Tuple<string, string>(book.UrlName, book.AnnotatorUrlName));
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

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            if (!values.ContainsKey("controller")) return null;
            if (values["controller"].ToString().ToLower() != "book") return null;
            if (!values.ContainsKey("id")) return null;
            int? bookId = values["id"] as int?;
            if (!bookId.HasValue) return null;
            if (bookDataIndex.TryGetValue(bookId.Value, out var bookData))
            {
                VirtualPathData basePath = base.GetVirtualPath(requestContext, values);
                return basePath;
            }
            return null;
        }

        public RouteData GetBookRouteData(HttpContextBase httpContext)
        {
            return base.GetRouteData(httpContext);
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

        public BookData GetBookData(RequestContext requestContext, RouteValueDictionary values)
        {
            if (!values.ContainsKey("id") || !(values["id"] is int bookId))
                return null;

            if (!bookDataIndex.TryGetValue(bookId, out var bookData))
                return null;

            string booktitle = bookData.Item1;

            string hostname = Domain.Replace("{booktitle}", booktitle);
            string protocol = requestContext.HttpContext.Request.Url.Scheme;

            // Return domain data
            return new BookData()
            {
                Protocol = protocol,
                HostName = hostname,
                Annotator = bookData.Item2
            };
        }
    }
}