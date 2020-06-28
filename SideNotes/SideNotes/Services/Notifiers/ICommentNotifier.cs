using SideNotes.Models;

namespace SideNotes.Services
{
    public interface ICommentNotifier
    {
        void NotifyNewComment(Comment newComment);
        void NotifyNewHeadComment(HeadComment newComment);
    }
}