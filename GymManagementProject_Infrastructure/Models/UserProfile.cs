using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class UserProfile : ISoftDelete
{
    public Guid UserId { get; set; }

    public byte[]? FullNameEnc { get; set; }

    public byte[]? PhoneEnc { get; set; }

    public string? PhoneHash { get; set; }

    public byte[]? IdentityCardNoEnc { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public byte[]? AddressFullEnc { get; set; }

    public byte[]? WardEnc { get; set; }

    public byte[]? DistrictEnc { get; set; }

    public byte[]? ProvinceEnc { get; set; }

    public string? DistrictHash { get; set; }

    public string? ProvinceHash { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual User User { get; set; } = null!;
}
