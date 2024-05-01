using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DeaneryManagementSystem.Services
{
    public class PersonService : IPersonService
    {
        private readonly DeaneryContext _context;

        public PersonService(DeaneryContext context)
        {
            _context = context;
        }

        public async Task<MethodResult> SavePersonAsync(Person person)
        {
            try
            {
                if (person.Id > 0)
                {
                    _context.Update(person);
                }
                else
                {
                    await _context.AddAsync(person);
                }
                await _context.SaveChangesAsync();
                return MethodResult.Success();
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public async Task<Person?> GetPersonById(int personId)
        {
            return await _context.People.FirstOrDefaultAsync(p => p.Id == personId);
        }

        public async Task<IEnumerable<Person>> GetTeachersAsync()
        {
            var result = await _context.People.Include(p => p.Group)
                                              .Include(p => p.MarkTeachers).ThenInclude(m => m.Subject)
                                              .Where(p => p.Type == 'P').ToListAsync();
            return result.OrderBy(result => result.Id);
        }

        public async Task<IEnumerable<Person>> GetStudentsAsync()
        {
            var result = await _context.People.Include(p => p.Group)
                                              .Include(p => p.MarkStudents).ThenInclude(m => m.Subject)
                                              .Where(p => p.Type == 'S').ToListAsync();
            return result.OrderBy(result => result.Id);
        }

        public async Task<List<PersonModel>> GetPersonModelsAsync(char groupType)
        {
            var baseList = await _context.People.Where(p => p.Type == groupType && p.GroupId != 9999).ToListAsync();
            return baseList.Select(p => new PersonModel { Id = p.Id, FullName = p.getFullName() }).ToList();
        }

        public async Task<MethodResult> DeletePersonAsync(int personId, bool isTeacher)
        {
            try
            {
                var personToDelete = await _context.People.Include(p => p.MarkStudents)
                                                           .Include(p => p.MarkTeachers)
                                                           .FirstOrDefaultAsync(p => p.Id == personId);
                if (personToDelete != null)
                {
                    if (isTeacher)
                    {
                        if (personToDelete.MarkTeachers.Any())
                        {
                            await _context.Marks.Where(mark => personToDelete.MarkTeachers.Contains(mark))
                                                .ForEachAsync(mark => mark.TeacherId = 9999);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        if (personToDelete.MarkStudents.Any())
                        {
                            var marksToDelete = await _context.Marks.Where(mark => personToDelete.MarkStudents.Contains(mark)).ToListAsync();
                            _context.Marks.RemoveRange(marksToDelete);
                            await _context.SaveChangesAsync();
                        }
                    }
                    _context.People.Remove(personToDelete);
                    await _context.SaveChangesAsync();
                    return MethodResult.Success();
                }
                return MethodResult.Failure($"Не найден человек с Id: {personId}");
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public void CheckEntries(Person person)
        {
            var personEntry = _context.Entry(person);
            if (personEntry.State == EntityState.Modified)
            {
                personEntry.CurrentValues.SetValues(personEntry.OriginalValues);
                personEntry.State = EntityState.Unchanged;
            }
        }
    }
}
