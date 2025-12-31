using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Checkin
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? MemberId { get; set; }

    public DateTime? CheckinAt { get; set; }

    public string? Method { get; set; }

    public string? DeviceId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Version { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
