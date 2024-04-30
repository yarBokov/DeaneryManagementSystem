using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DeaneryManagementSystem.Services
{
    public class GroupService : IGroupService
    {
        private readonly DeaneryContext _context;

        public GroupService(DeaneryContext context)
        {
            _context = context;
        }

        public async Task<MethodResult> SaveGroupAsync(Group group)
        {
            try
            {
                if (group.Id > 0)
                {
                    _context.Update(group);
                }
                else
                {
                    var foundObj = await _context.Groups.FirstOrDefaultAsync(g => g.Name == group.Name && g.Year == group.Year);
                    if (foundObj != null)
                    {
                        return MethodResult.Failure("Группа с таким годом и таким названием уже существует!");
                    }
                    await _context.AddAsync(group);
                }
                await _context.SaveChangesAsync();
                return MethodResult.Success();
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public async Task<IEnumerable<Group>> GetGroupsAsync()
        {
            var result = await _context.Groups.Include(g => g.People).Where(g => g.Name != null && g.Id != 8888 && g.Id != 9999).ToListAsync();
            return result.OrderBy(group => group.Id);
        }

        public async Task<Group> GetGroupById(int groupId) => await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

        public async Task<IEnumerable<Group>> GetGroupsByTypeAsync(char type) => await _context.Groups.Include(g => g.People)
                     .Where(g => g.People.FirstOrDefault().Type == type &&
                     (type == 'P' ? g.People.FirstOrDefault().GroupId != 9999 : true)).ToListAsync();


        public async Task<MethodResult> DeleteGroupAsync(int groupId)
        {
            try
            {
                var result = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (result != null)
                {
                    _context.Groups.Remove(result);
                    await _context.SaveChangesAsync();
                    return MethodResult.Success();
                }
                return MethodResult.Failure($"Не найдена группа с Id: {groupId}");
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public IEnumerable<int> GenerateTermSeq(int groupId) =>
            Enumerable.Range(1, _context.Groups.FirstOrDefault(g => g.Id == groupId).getMaxTerm());
    }
}
