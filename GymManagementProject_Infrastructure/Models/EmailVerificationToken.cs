using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class EmailVerificationToken
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? MemberId { get; set; }

    public string Email { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public string TokenType { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? SentCount { get; set; }

    public string? CreatedByIp { get; set; }

    public virtual Member? Member { get; set; }

    public virtual User? User { get; set; }
}
