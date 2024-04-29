using System;
using System.Collections.Generic;

namespace DeaneryManagementSystem.Data.Entities;

public partial class Key
{
    public int Id { get; set; }

    public string? AccessKey { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
