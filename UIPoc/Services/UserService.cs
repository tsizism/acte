using Microsoft.EntityFrameworkCore;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class UserService : IUserService
    {
        private readonly HoldingsDbContext _context;

        public UserService(HoldingsDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            // For now, return the first user from the database
            // In a real application, this would get the authenticated user
            return await _context.Users.FirstOrDefaultAsync();
        }
    }
}
