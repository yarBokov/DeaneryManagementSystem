using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Models;
using DeaneryManagementSystem.Authentication;
using Microsoft.EntityFrameworkCore;

namespace DeaneryManagementSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly DeaneryContext _context;

        public AccountService(DeaneryContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByPersonId(int personId) =>
            await _context.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.PersonId == personId);

        public async Task<MethodResult> SaveUserAsync(User user)
        {
            try
            {
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();
                return MethodResult.Success();
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public async Task<Key?> GetKeyAsync(string accessKey, PasswordHasher hasher)
        {
            var keysList = await _context.Keys.ToListAsync();
            var key = keysList.FirstOrDefault(k => hasher.Verify(k.AccessKey, accessKey));
            return key;
        }
    }
}
