namespace SideNotes.Services
{
    public interface ICommentManager
    {
        void AddComment(int AuthorId, int? parentCommentId, int headCommentId, string commentText);
        void AddHeadComment(int AuthorId, int entityId, int entityType, string commentText, bool isPrivate);
        void PublishTemporaryComment(int tempId, int authorId);
        int SaveTemporaryComment(int entityId, int entityType, string commentText, bool isPrivate);
    }
}