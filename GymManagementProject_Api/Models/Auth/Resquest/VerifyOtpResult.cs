public class VerifyOtpResult
{
    public Guid UserId { get; set; }
    public Guid? MemberId { get; set; }
    public string Purpose { get; set; } = null!;
}
