using System.ComponentModel.DataAnnotations;

public class VerifyEmailDto
{
    public string Email { get; set; } = null!;

    public string OTP { get; set; } = null!;
}
