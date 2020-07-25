namespace SideNotes.Repositories
{
    public interface IUserRepository
    {
        bool IsUrlNameAvailable(int userId, string urlName);
    }
}