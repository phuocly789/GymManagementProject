using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public string? LastUsedIp { get; set; }

    public string? DeviceInfo { get; set; }

    public int? Version { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public bool IsUsed { get; set; }

    public string? RevokedByIp { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RefreshToken> InverseReplacedByToken { get; set; } = new List<RefreshToken>();

    public virtual RefreshToken? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;
}
