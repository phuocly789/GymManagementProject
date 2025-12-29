using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Trainer
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Bio { get; set; }

    public List<string>? Specialties { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<PtContract> PtContracts { get; set; } = new List<PtContract>();

    public virtual User? User { get; set; }
}
