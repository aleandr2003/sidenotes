using System.Collections.Generic;
using SideNotes.Models;

namespace SideNotes.Repositories
{
    public interface IBookRepository
    {
        List<Book> GetBooksWithDomainData();
    }
}