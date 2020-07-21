using SideNotes.Models;

namespace SideNotes.Services
{
    public interface ICommentNotifier
    {
        void NotifyNewComment(Comment newComment, string absoluteUrl);
        void NotifyNewHeadComment(HeadComment newComment, string absoluteUrl);
    }
}