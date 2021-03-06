﻿using SideNotes.Controllers;
using SideNotes.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SideNotes.Application
{
    public class BookRoute : DomainRoute
    {
        private static ConcurrentDictionary<string, int> bookIndex;
        private static ConcurrentDictionary<int, Tuple<string, string>> bookDataIndex;
        private static HashSet<string> controllers;
        private static HashSet<string> bookActions;

        static BookRoute()
        {
            bookIndex = new ConcurrentDictionary<string, int>();
            bookDataIndex = new ConcurrentDictionary<int, Tuple<string, string>>();

            Assembly asm = Assembly.GetExecutingAssembly();
            var controllerNames = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .Select(type => type.Name.EndsWith("Controller") ? type.Name.Substring(0, type.Name.Length - "Controller".Length) : type.Name);

            var bookActionNames = asm.GetTypes()
                .Where(type => typeof(BookController).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods())
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)))
                .Select(method => method.Name);

            controllers = new HashSet<string>(controllerNames);
            bookActions = new HashSet<string>(bookActionNames);
            if (controllers.Intersect(bookActions).Any())
            {
                throw new Exception("Inconclusive routing");
            }
        }

        public static void Init(IBookRepository repository)
        {
            var books = repository.GetBooksWithDomainData();
            bookIndex.Clear();

            var booksWithoutAnnotators = books.Where(b => String.IsNullOrEmpty(b.AnnotatorUrlName)).ToList();
            var booksWithAnnotators = books.Where(b => !String.IsNullOrEmpty(b.AnnotatorUrlName)).ToList();

            booksWithoutAnnotators.ForEach(book =>
            {
                bookIndex.TryAdd(CreateKey(book.UrlName, String.Empty), book.Id);
                bookDataIndex.TryAdd(book.Id, new Tuple<string, string>(book.UrlName, book.AnnotatorUrlName));
            });

            booksWithAnnotators.ForEach(book =>
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
                VirtualPathData basePath = base.GetVirtualPath(requestContext, RemoveTokens(values, "controller", "id"));
                return basePath;
            }
            return null;
        }

        private RouteValueDictionary RemoveTokens(RouteValueDictionary allTokens, params string[] tokens)
        {
            RouteValueDictionary remainingTokens = new RouteValueDictionary();
            var tokensLowerCase = tokens.Select(t => t.ToLower()).ToHashSet();
            foreach (var token in allTokens)
            {
                if (tokensLowerCase.Contains(token.Key.ToLower())) continue;
                remainingTokens.Add(token.Key, token.Value);
            }
            return remainingTokens;
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

            if (data.Values.ContainsKey("action") && controllers.Contains(data.Values["action"]))
                return null;

            if(bookIndex.TryGetValue(bookKey, out int bookId))
            {
                data.Values["controller"] = "Book";
                data.Values["Id"] = bookId;
                return data;
            }
            return null;
        }

        public BookData GetBookData(HttpContextBase httpContext, RouteValueDictionary values)
        {
            if (!values.ContainsKey("id") || !(values["id"] is int bookId))
                return null;

            if (!bookDataIndex.TryGetValue(bookId, out var bookData))
                return null;

            string booktitle = bookData.Item1;

            string hostname = Domain.Replace("{booktitle}", booktitle);
            string protocol = httpContext.Request.Url.Scheme;

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