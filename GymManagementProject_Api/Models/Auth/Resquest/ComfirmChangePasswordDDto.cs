using System.ComponentModel.DataAnnotations;

public class ConfirmChangePasswordDto
{
    public string? Email { get; set; } = null!;
    public string OTP { get; set; } = null!;
    public string NewPassword { get; set; } = null!;

    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = null!;
}
