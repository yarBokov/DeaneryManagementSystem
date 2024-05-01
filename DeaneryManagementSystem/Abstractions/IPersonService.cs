using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;

namespace DeaneryManagementSystem.Abstractions
{
    public interface IPersonService
    {
        Task<Person?> GetPersonById(int personId);
        Task<MethodResult> SavePersonAsync(Person person);
        Task<MethodResult> DeletePersonAsync(int personId, bool isTeacher);
        void CheckEntries(Person person);
        Task<IEnumerable<Person>> GetTeachersAsync();
        Task<IEnumerable<Person>> GetStudentsAsync();
        Task<List<PersonModel>> GetPersonModelsAsync(char groupType);
    }
}