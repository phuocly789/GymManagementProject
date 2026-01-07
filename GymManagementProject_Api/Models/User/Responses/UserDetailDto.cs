public class UserDetailDto : IPiiTarget
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool? IsActive { get; set; }

    public string? Phone { get; set; }
    public string? IdentityCardNo { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }

    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();

    public List<string> AccessibleBranches { get; set; } = new();
}
