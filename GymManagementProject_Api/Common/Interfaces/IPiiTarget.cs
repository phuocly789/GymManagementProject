public interface IPiiTarget
{
    string? Phone { get; set; }
    string? IdentityCardNo { get; set; }
    string? Address { get; set; }
    DateOnly? DateOfBirth { get; set; }
    string? Gender { get; set; }
}
