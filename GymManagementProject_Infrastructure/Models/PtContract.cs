using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class PtContract
{
    public Guid Id { get; set; }

    public Guid? MemberId { get; set; }

    public Guid? TrainerId { get; set; }

    public int TotalSessions { get; set; }

    public int? UsedSessions { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Trainer? Trainer { get; set; }
}
