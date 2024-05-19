using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;

namespace DeaneryManagementSystem.Abstractions
{
    public interface ISubjectService
    {
        Task<MethodResult> DeleteSubjectAsync(int subjectId);
        Task<Subject> GetSubjectById(int subjectId);
        Task<IEnumerable<Subject>> GetSubjectsAsync();
        Task<MethodResult> SaveSubjectAsync(Subject subject);
    }
}