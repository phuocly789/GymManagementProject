using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string? Action { get; set; }

    public string? TableName { get; set; }

    public Guid? RecordId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }
}
