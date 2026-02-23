using UIPooc.Models;

namespace UIPooc.Services
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync();
    }
}
