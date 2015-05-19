using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Helpers
{
    public class BookIndexBuilder
    {
        public static Chapter BuildIndex(List<Chapter> chapters)
        {
            Stack<Chapter> parents = new Stack<Chapter>();
            List<Chapter> orderedChapters = chapters.OrderBy(c => c.OrderNumber).ToList();
            orderedChapters.ForEach(c => c.ChildChapters = new List<Chapter>());
            Chapter RootChapter = orderedChapters.First();
            parents.Push(RootChapter);
            foreach (Chapter chapter in orderedChapters.Skip(1))
            {
                while (parents.Count > 0 && parents.Peek().Id != chapter.ParentChapter_Id)
                {
                    parents.Pop();
                }
                if (parents.Count > 0) parents.Peek().ChildChapters.Add(chapter);
                parents.Push(chapter);
            }

            return RootChapter;
        }

    }
}