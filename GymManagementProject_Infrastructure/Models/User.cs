using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class User
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public bool? IsActive { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } =
        new List<EmailVerificationToken>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();

    public virtual ICollection<UserBranchAccess> UserBranchAccesses { get; set; } =
        new List<UserBranchAccess>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
