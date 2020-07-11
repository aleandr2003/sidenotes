namespace SideNotes.Services
{
    public interface IBookAuthorNotifier
    {
        void CreateDailyDigest(int bookId);
    }
}