using System;
using System.Collections.Generic;

namespace DeaneryManagementSystem.Data.Entities;

public partial class Mark
{
    public int Id { get; set; }

    public int? StudentId { get; set; }

    public int? SubjectId { get; set; }

    public int? TeacherId { get; set; }

    public int? Value { get; set; }

    public int? Term { get; set; }

    public virtual Person? Student { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual Person? Teacher { get; set; }
}
