using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Branch
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<UserBranchAccess> UserBranchAccesses { get; set; } = new List<UserBranchAccess>();
}
