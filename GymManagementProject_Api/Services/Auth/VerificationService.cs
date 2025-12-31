using dotnet03WebApi_EbayProject.Helper;
using GymManagementProject_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IVerificationService
{
    Task<string> SendOtpAsync(
        string email,
        Guid tenantId,
        string purpose,
        Guid userId,
        Guid? memberId = null,
        string? templateCode = null,
        Dictionary<string, string>? extraParameters = null
    );

    Task<VerifyOtpResult> VerifyOtpAsync(string email, string otp, string purpose);
}

public class VerificationService : IVerificationService
{
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxAttemptsPer15Min = 3;
    private const int MaxAttemptsPerHour = 6;
    private const int OtpExpiryMinutes = 3;

    public VerificationService(
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork
    )
    {
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> SendOtpAsync(
        string email,
        Guid tenantId,
        string purpose,
        Guid userId,
        Guid? memberId = null,
        string? templateCode = null,
        Dictionary<string, string>? extraParameters = null
    )
    {
        string normalizedEmail = email.ToLower().Trim();

        //chống spam: kiểm tra số lần gửi gần đây theo purpose
        var cutoff15Min = DateTime.UtcNow.AddMinutes(-15);
        var recentCount = await _emailVerificationTokenRepository
            .Query()
            .CountAsync(t =>
                t.Email == normalizedEmail && t.Purpose == purpose && t.CreatedAt >= cutoff15Min
            );

        if (recentCount >= MaxAttemptsPer15Min)
        {
            throw new BadRequestException(
                "Bạn đã gửi quá nhiều yêu cầu xác thực trong 15 phút qua. Vui lòng thử lại sau."
            );
        }

        var cutoff1Hour = DateTime.UtcNow.AddHours(-1);
        var recentHourCount = await _emailVerificationTokenRepository
            .Query()
            .CountAsync(t =>
                t.Email == normalizedEmail && t.Purpose == purpose && t.CreatedAt >= cutoff1Hour
            );

        if (recentHourCount >= MaxAttemptsPerHour)
        {
            throw new BadRequestException(
                "Bạn đã gửi quá nhiều yêu cầu xác thực trong 1 giờ qua. Vui lòng thử lại sau."
            );
        }

        //Tạo mã OTP mới
        string otp = GenerateOTP();

        //vô hiệu hóa toke cũ
        var oldTokens = await _emailVerificationTokenRepository
            .Query()
            .Where(t => t.Email == normalizedEmail && t.Purpose == purpose && t.UsedAt == null)
            .ToListAsync();

        foreach (var token in oldTokens)
        {
            token.UsedAt = DateTime.UtcNow;
            await _emailVerificationTokenRepository.Update(token);
        }

        //lưu token mới
        var newToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MemberId = memberId,
            Email = normalizedEmail,
            TokenHash = PasswordHelper.HashPassword(otp),
            TokenType = "otp",
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            CreatedAt = DateTime.UtcNow,
            SentCount = oldTokens.Any() ? oldTokens.Max(t => t.SentCount) + 1 : 1,
        };

        await _emailVerificationTokenRepository.AddAsync(newToken);
        await _unitOfWork.SaveChangesAsync();

        //LẤY THÔNG TIN ĐỂ RÉNDER EMAIL
        var parameters = new Dictionary<string, string>
        {
            { "OTP", otp },
            { "ExpiryMinutes", OtpExpiryMinutes.ToString() },
            { "PurposeTitle", GetPurposeTitle(purpose) },
            { "ActionDescription", GetActionDescription(purpose) },
            { "AppName", "Gym" }, // Có thể lấy từ config sau
            { "CurrentYear", DateTime.UtcNow.Year.ToString() },
        };

        if (extraParameters != null)
        {
            foreach (var kvp in extraParameters)
            {
                parameters[kvp.Key] = kvp.Value;
            }
        }

        templateCode ??= purpose switch
        {
            "sigup" => "REGISTRATION_OTP",
            "reset_password" => "COMMON_OTP_VERIFICATION",
            "change_password" => "COMMON_OTP_VERIFICATION",
            "verify_account" => "COMMON_OTP_VERIFICATION",
            _ => "DEFAULT_OTP",
        };

        await _emailService.SendTemplateAsync(templateCode, tenantId, parameters, normalizedEmail);

        return otp;
    }

    public async Task<VerifyOtpResult> VerifyOtpAsync(string email, string otp, string purpose)
    {
        string normalizedEmail = email.ToLower().Trim();

        var tokenRecord = await _emailVerificationTokenRepository
            .Query()
            .Where(t =>
                t.Email == normalizedEmail
                && t.Purpose == purpose
                && t.UsedAt == null
                && t.ExpiresAt >= DateTime.UtcNow
            )
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (tokenRecord == null)
            throw new BadRequestException("Mã OTP đã hết hạn. Vui lòng yêu cầu OTP mới.");

        if (!PasswordHelper.VerifyPassword(otp, tokenRecord.TokenHash))
        {
            throw new BadRequestException("Mã OTP không hợp lệ");
        }

        // Đánh dấu token là đã sử dụng
        tokenRecord.UsedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return new VerifyOtpResult
        {
            UserId = tokenRecord.UserId,
            MemberId = tokenRecord.MemberId,
            Purpose = tokenRecord.Purpose,
        };
    }

    private string GenerateOTP()
    {
        return Random.Shared.Next(100000, 999999).ToString("D6");
    }

    private string GetPurposeTitle(string purpose) =>
        purpose switch
        {
            "sigup" => "Đăng ký hội viên",
            "reset_password" => "Quên mật khẩu",
            "change_password" => "Đổi mật khẩu",
            "verify_account" => "Xác thực tài khoản",
            _ => "Xác thực tài khoản",
        };

    private string GetActionDescription(string purpose)
    {
        return purpose switch
        {
            "signup" => "xác thực tài khoản mới",
            "reset_password" => "đặt lại mật khẩu",
            "change_password" => "đổi mật khẩu",
            "sensitive_action" => "thực hiện hành động nhạy cảm",
            _ => "xác thực bảo mật",
        };
    }
}
