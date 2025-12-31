public class UserResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? Version { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}
