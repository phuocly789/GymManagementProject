using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class MessageTemplate
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string? Subject { get; set; }

    public string? BodyTemplate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
