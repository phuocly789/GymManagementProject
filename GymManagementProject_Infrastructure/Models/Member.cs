using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Member
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? HomeBranchId { get; set; }

    public string MemberCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();

    public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public virtual Branch? HomeBranch { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual MemberProfile? MemberProfile { get; set; }

    public virtual ICollection<PtContract> PtContracts { get; set; } = new List<PtContract>();

    public virtual Tenant Tenant { get; set; } = null!;
}
