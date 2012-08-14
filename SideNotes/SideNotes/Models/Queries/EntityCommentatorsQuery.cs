using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Queries
{
    public class EntityCommentatorsQuery
    {
        int? currentUserId;
        int entityId;
        int entityType;
        int take;
        List<int> userFilter;

        public EntityCommentatorsQuery(int? currentUserId, int entityId, int entityType, int take, List<int> filter)
        {
            this.currentUserId = currentUserId;
            this.entityId = entityId;
            this.entityType = entityType;
            this.take = take;
            this.userFilter = new List<int>();
            this.userFilter.AddRange(filter);
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

            var headCommentators = (from h in context.HeadComments
                                    join u in context.Users
                                    on h.Author_Id equals u.Id
                                    where h.EntityId == entityId && h.EntityType == entityType
                                    && userFilter.Contains(h.Author_Id ?? -1)
                                    && h.Author_Id != null && (!h.IsPrivate || h.Author_Id == currentUserId)
                                    select new { Id = u.Id, priority = u.IsFamous ? 1 : 0 }
                                    ).Distinct().ToList();
            //var threadCommentators = (from h in context.HeadComments
            //                          join c in context.Comments
            //                          on h.Id equals c.HeadCommentId
            //                          join u in context.Users
            //                          on c.Author_Id equals u.Id
            //                          where h.EntityId == entityId && h.EntityType == entityType
            //                          && (!h.IsPrivate || h.Author_Id == currentUserId)
            //                          && c.Author_Id != null
            //                          select new { Id = u.Id, priority = u.IsFamous ? 1 : 0 })
            //                          .Distinct().ToList();
            var commentators = headCommentators
                //.Concat(threadCommentators).Distinct()
                .Select(c => new UserSelector() { Id = c.Id, priority = c.priority, randomOrder = 0 }).ToList();

            commentators.ForEach(u => u.priority = (friendIds.Contains(u.Id) ? 2 : u.priority));
            Random random = new Random();
            commentators.ForEach(u => u.randomOrder = random.Next());
            return commentators.OrderByDescending(c => c.priority).ThenBy(c => c.randomOrder)
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