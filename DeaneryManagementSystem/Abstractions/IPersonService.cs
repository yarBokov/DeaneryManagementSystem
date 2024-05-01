using DeaneryManagementSystem.Data.Entities;

namespace DeaneryManagementSystem.Abstractions
{
    public interface IPersonService
    {
        Task<Person?> GetPersonById(int personId);
    }
}