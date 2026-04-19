using Concert.Models;
using Microsoft.EntityFrameworkCore;

namespace Concert.Services
{
    public class UserService : IUserService
    {
        private readonly ConcertContext _context;

        public UserService(ConcertContext context)
        {
            _context = context;
        }

        public async Task<string> GetAvatarUrlAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return null;

           
            var avatarUrl = await _context.Customers
                .Where(c => c.Email == userName || c.FullName == userName)
                .Select(c => c.AvatarUrl)
                .FirstOrDefaultAsync();

            return avatarUrl;
        }
    }
}