using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class NotificationLog
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? RecipientId { get; set; }

    public string? Channel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
