using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeaneryManagementSystem.Services
{
    public class PersonService : IPersonService
    {
        private readonly DeaneryContext _context;

        public async Task<Person?> GetPersonById(int personId)
        {
            return await _context.People.FirstOrDefaultAsync(p => p.Id == personId);
        }
    }
}
