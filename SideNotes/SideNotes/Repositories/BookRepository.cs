using SideNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Repositories
{
    public class BookRepository : IBookRepository
    {
        public List<Book> GetBooksWithDomainData()
        {
            using (var context = new SideNotesEntities())
            {
                return context.Books.Where(u => u.UrlName != null && u.UrlName != String.Empty).ToList();
            }
        }
    }
}