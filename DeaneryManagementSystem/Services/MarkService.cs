using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Data.Entities;
using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DeaneryManagementSystem.Services
{
    public class MarkService : IMarkService
    {
        private readonly DeaneryContext _context;
        public MarkService(DeaneryContext context)
        {
            _context = context;
        }

        public async Task<MethodResult> SaveMarkAsync(Mark mark)
        {
            try
            {
                if (mark.Id > 0)
                {
                    _context.Update(mark);
                }
                else
                {
                    await _context.AddAsync(mark);
                }
                await _context.SaveChangesAsync();
                return MethodResult.Success();
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }

        public async Task<IEnumerable<Mark>> GetMarksAsync()
        {
            var result = await _context.Marks.Include(m => m.Teacher)
                                       .Include(m => m.Subject).ThenInclude(s => s.Marks)
                                       .Include(m => m.Student).ThenInclude(s => s.Group).ToListAsync();
            return result.OrderBy(mark => mark.Student.GroupId).ThenBy(mark => mark.Student.SecondName).ThenBy(mark => mark.Subject.Name);
        }

        public IEnumerable<int> GetFreeTermsToEdit(Mark mark)
        {
            List<int> terms = _context.Marks.Where(m => m.StudentId == mark.StudentId &&
                m.SubjectId == mark.SubjectId).Select(m => m.Term.Value).ToList();
            terms.Remove(mark.Term.Value);
            List<int> untrackedTerms = Enumerable.Range(1, mark.Student.Group.getMaxTerm()).Except(terms).ToList();
            return untrackedTerms.OrderBy(i => i);
        }

        public async Task<MethodResult> DeleteMarkAsync(int markId)
        {
            try
            {
                var result = await _context.Marks.FirstOrDefaultAsync(m => m.Id == markId);
                if (result != null)
                {
                    _context.Marks.Remove(result);
                    await _context.SaveChangesAsync();
                    return MethodResult.Success();
                }
                return MethodResult.Failure($"Не найдена оценка с Id: {markId}");
            }
            catch (Exception ex)
            {
                return MethodResult.Failure(ex.Message);
            }
        }
        public void CheckEntries(Mark mark)
        {
            var markEntry = _context.Entry(mark);
            if (markEntry.State == EntityState.Modified)
            {
                markEntry.CurrentValues.SetValues(markEntry.OriginalValues);
                markEntry.State = EntityState.Unchanged;
            }
        }

        public List<MarkDistrModel> GetMarkDistrs()
        {
            List<MarkDistrModel> markDistrs = new List<MarkDistrModel>();
            int marksCount = 0;
            for (int i = 2; i < 6; i++)
            {
                foreach (var subject in _context.Subjects.Include(s => s.Marks).Where(s => s.Name != null).ToList())
                {
                    foreach (var group in _context.Groups.Include(g => g.People).Where(g => g.People.FirstOrDefault().Type == 'S').ToList())
                    {
                        for (int j = 1; j < group.getMaxTerm() + 1; j++)
                        {
                            marksCount = subject.Marks.Where(m =>
                                    m.Term == j && m.Value == i && group.People.Select(g => g.Id).ToList().Contains(m.StudentId.Value)).Count();
                            if (marksCount > 0)
                                markDistrs.Add(new MarkDistrModel
                                {
                                    Term = j,
                                    MarkNumber = i.ToString(),
                                    GroupId = group.Id,
                                    SubjectId = subject.Id,
                                    MarkCount = marksCount,
                                });
                        }
                    }
                }
            }
            return markDistrs;
        }
        public List<AvgMarkTermModel> GetAvgMarkTermModels()
        {
            List<AvgMarkTermModel> avgMarks = new List<AvgMarkTermModel>();
            foreach (var subject in _context.Subjects.Include(s => s.Marks).Where(s => s.Name != null).ToList())
            {
                foreach (var group in _context.Groups.Include(g => g.People).Where(g => g.People.FirstOrDefault().Type == 'S').ToList())
                {
                    for (int i = 1; i < group.getMaxTerm() + 1; i++)
                    {
                        if (subject.Marks.Where(m =>
                                    m.Term == i && group.People.Select(g => g.Id).ToList().Contains(m.StudentId.Value)).Any())
                            avgMarks.Add(new AvgMarkTermModel
                            {
                                Term = i.ToString() + " сем.",
                                GroupId = group.Id,
                                SubjectId = subject.Id,
                                AvgMark = subject.getAverageMarkByGroup(group.Id, i)
                            });
                    }
                }
            }
            return avgMarks;
        }
    }
}
