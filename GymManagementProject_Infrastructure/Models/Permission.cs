using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Permission
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Module { get; set; }

    public virtual Tenant? Tenant { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
