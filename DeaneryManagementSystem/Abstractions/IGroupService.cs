using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;

namespace DeaneryManagementSystem.Abstractions
{
    public interface IGroupService
    {
        Task<IEnumerable<Group>> GetGroupsAsync();
        Task<MethodResult> SaveGroupAsync(Group group);
        Task<Group> GetGroupById(int groupId);
        Task<MethodResult> DeleteGroupAsync(int personId);
        Task<IEnumerable<Group>> GetGroupsByTypeAsync(char type);
        IEnumerable<int> GenerateTermSeq(int groupId);
    }
}