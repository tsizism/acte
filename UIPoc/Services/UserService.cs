using Microsoft.EntityFrameworkCore;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class UserService : IUserService
    {
        private User? _currentUser;
        private readonly IModelService _modelService;

        public UserService(IModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser == null)
            {
                _currentUser = await _modelService.GetCurrentUserAsync();
            }

            // For now, return the first user from the database
            // In a real application, this would get the authenticated user
            return _currentUser;
        }
    }
}
