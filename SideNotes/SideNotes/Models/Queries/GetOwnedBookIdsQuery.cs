using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Queries
{
    public class GetOwnedBookIdsQuery
    {
        private int userId;
        public GetOwnedBookIdsQuery(int userId)
        {
            this.userId = userId;
        }

        public List<int> Load(SideNotesEntities context)
        {
            return (from b in context.Books
                            join a in context.Autosaves
                                on b.Id equals a.Book_Id
                            where a.Owner_Id == userId
                            select b.Id).Distinct().ToList();
        }
    }
}