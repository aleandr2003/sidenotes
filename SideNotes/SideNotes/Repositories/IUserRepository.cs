using SideNotes.ViewModels;

namespace SideNotes.Repositories
{
    public interface IUserRepository
    {
        bool IsUrlNameAvailable(int userId, string urlName);

        void UpdateSettings(int userId, EditSettingsModel settings);
    }
}