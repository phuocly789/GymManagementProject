using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class MemberProfile
{
    public Guid MemberId { get; set; }

    public byte[]? FullNameEnc { get; set; }

    public byte[]? PhoneEnc { get; set; }

    public byte[]? EmailEnc { get; set; }

    public string? PhoneHash { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual Member Member { get; set; } = null!;
}
