public class AuthRegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; }
    public string Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public Guid HomeBranchId { get; set; }
    public AddressDto Address { get; set; }
}

public class AddressDto
{
    public string? Detail { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? Full { get; set; }
}

public enum Gender
{
    Male,
    Female,
    Other,
}
