using System;
using System.Collections.Generic;

namespace DeaneryManagementSystem.Data.Entities;

public partial class Subject
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public double getAverageMark() =>
    getAverageMarkInList(Marks);

    public double getAverageMarkByGroup(int groupId) =>
        getAverageMarkInList(Marks.Where(m => m.Student.GroupId == groupId));

    public double getAverageMarkByGroup(int groupId, int term) =>
        getAverageMarkInList(Marks.Where(m => m.Student.GroupId == groupId && m.Term == term));

    public double getAverageMarkInTermRange(params int[] terms) =>
        getAverageMarkInList(Marks.Where(m => Array.IndexOf(terms, m.Term) != -1));

    private double getAverageMarkInList(IEnumerable<Mark> marks)
    {
        double markSum = 0.0;
        foreach (var mark in marks)
        {
            markSum += mark.Value.Value;
        }
        return markSum / marks.Count();
    }
}
