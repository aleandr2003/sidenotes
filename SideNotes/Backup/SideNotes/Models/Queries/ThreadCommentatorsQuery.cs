using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Queries
{
    public class ThreadCommentatorsQuery
    {
        int? currentUserId;
        int headCommentId;
        int take;

        public ThreadCommentatorsQuery(int? currentUserId, int headCommentId, int take)
        {
            this.currentUserId = currentUserId;
            this.headCommentId = headCommentId;
            this.take = take;
        }

        public List<int> Load(SideNotesEntities context)
        {
            List<int> friendIds = new List<int>();
            if (currentUserId != null)
            {
                friendIds.AddRange(context.Users
                    .First(u => u.Id == currentUserId)
                    .Friends
                    .Select(f => f.Id));
            }
            var threadCommentators = (from c in context.Comments
                                      join u in context.Users
                                      on c.Author_Id equals u.Id
                                      where c.HeadCommentId == headCommentId
                                      && c.Author_Id != null
                                      select new { Id = u.Id, priority = u.IsFamous ? 1 : 0})
                                      .Distinct().ToList()
                                      .Select(c => new UserSelector() { Id = c.Id, priority = c.priority, randomOrder = 0 })
                                      .ToList();
            threadCommentators.ForEach(u => u.priority = (friendIds.Contains(u.Id) ? 2 : u.priority));
            Random random = new Random();
            threadCommentators.ForEach(u => u.randomOrder = random.Next());
            return threadCommentators.OrderBy(c => c.priority).ThenBy(c => c.randomOrder)
                .Take(take).Select(c => c.Id).ToList();
        }

        public struct UserSelector
        {
            public int Id;
            public int priority;
            public int randomOrder;
        }
    }
}