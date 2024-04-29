using System;
using System.Collections.Generic;

namespace DeaneryManagementSystem.Data.Entities;

public partial class Group
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Year { get; set; }

    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
