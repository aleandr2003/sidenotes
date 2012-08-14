using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Queries
{
    public class EntityListCommentatorsQuery
    {
        int? currentUserId;
        List<int> entityIds;
        int entityType;
        int take;
        List<int> userFilter;

        public EntityListCommentatorsQuery(int? currentUserId, List<int> entityIds, int entityType, int take, List<int> filter)
        {
            this.currentUserId = currentUserId;
            this.entityIds = new List<int>();
            this.entityIds.AddRange(entityIds);
            this.entityType = entityType;
            this.take = take;
            this.userFilter = new List<int>();
            if (filter != null) this.userFilter.AddRange(filter);
        }

        public Dictionary<int, List<int>> Load(SideNotesEntities context)
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
                                    where entityIds.Contains(h.EntityId) && h.EntityType == entityType
                                    && userFilter.Contains(h.Author_Id ?? -1)
                                    && h.Author_Id != null && (!h.IsPrivate || h.Author_Id == currentUserId)
                                    select new { EntityId = h.EntityId, Id = u.Id, priority = u.IsFamous ? 1 : 0 }
                                    ).Distinct().ToList();

            var toSortDic = new Dictionary<int, List<UserSelector>>();
            Random random = new Random();
            foreach (var commentator in headCommentators)
            {
                if (!toSortDic.ContainsKey(commentator.EntityId))
                {
                    toSortDic[commentator.EntityId] = new List<UserSelector>();
                }
                toSortDic[commentator.EntityId].Add(new UserSelector() { 
                    Id = commentator.Id, 
                    priority = (friendIds.Contains(commentator.Id) ? 2 : commentator.priority),
                    randomOrder = random.Next()
                });
            }
            return toSortDic.ToDictionary(
                i => i.Key, 
                i => i.Value.OrderByDescending(c => c.priority).ThenBy(c => c.randomOrder).Take(take).Select(c => c.Id).ToList());
        }

        public struct UserSelector
        {
            public int Id;
            public int priority;
            public int randomOrder;
        }
    }
}