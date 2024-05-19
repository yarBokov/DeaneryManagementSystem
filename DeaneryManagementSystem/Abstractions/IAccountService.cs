using DeaneryManagementSystem.Authentication;
using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;

namespace DeaneryManagementSystem.Abstractions
{
    public interface IAccountService
    {
        Task<User?> GetUserByPersonId(int personId);
        Task<MethodResult> SaveUserAsync(User user);
        Task<Key?> GetKeyAsync(string accessKey, PasswordHasher hasher);
    }
}