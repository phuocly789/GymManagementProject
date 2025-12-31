using System.Security.Cryptography;
using System.Text;
using dotnet03WebApi_EbayProject.Helper;
using GymManagementProject_Infrastructure.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public interface IAuthService
{
    Task<AuthResponseDto> Login(AuthLoginDto login, string ipAddress, string? deviceInfo);
    Task<string> Register(AuthRegisterDto register);
    Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
    Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        string? deviceInfo
    );
    Task<string> CreateRefreshTokenAsync(User user, string ipAddress, string? deviceInfo);
    Task ChangePasswordAsync(Guid userId, RequestChangePasswordDto dto);
    Task ConfirmChangePasswordAsync(Guid userId, ConfirmChangePasswordDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IMemberProfileRepository _memberProfileRepository;
    private readonly JwtAuthService _jwtAuthService;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IVerificationService _verificationService;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public AuthService(
        IUserRepository repository,
        JwtAuthService jwtAuthService,
        IUnitOfWork unitOfWork,
        IMemberRepository memberRepository,
        IBranchRepository branchRepository,
        IRoleRepository roleRepository,
        IEmailService emailService,
        IVerificationService verificationService,
        IRefreshTokenRepository refreshTokenRepository,
        IEncryptionService encryptionService,
        IMemberProfileRepository memberProfileRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration
    )
    {
        _memberRepository = memberRepository;
        _branchRepository = branchRepository;
        _roleRepository = roleRepository;
        _memberProfileRepository = memberProfileRepository;
        _emailService = emailService;
        _refreshTokenRepository = refreshTokenRepository;
        _verificationService = verificationService;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
        _jwtAuthService = jwtAuthService;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _userRepository = repository;
        _httpContextAccessor = httpContextAccessor;
        _accessTokenExpiryMinutes = int.Parse(
            configuration["JwtConfigs:AccessTokenExpiryMinutes"] ?? "60"
        );
        _refreshTokenExpiryDays = int.Parse(
            configuration["JwtConfigs:RefreshTokenExpiryDays"] ?? "30"
        );
    }

    // Client (Mobile/Web)          Server (ASP.NET Core)          Database
    // -----------------            ---------------------          --------

    // 1. Login Request ------------> Validate Credentials
    //                                Issue Access Token + New Refresh Token
    //                                Store New Refresh: {Token: RT1, IsUsed: false, Expires: +30d}
    // <------------ Access + RT1

    // (Time passes, Access expires)

    // 2. Refresh Request (RT1) ----> Find RT1 in DB
    //                                If Expired or Revoked or IsUsed=true:
    //                                  - If IsUsed=true: Detect Reuse -> Revoke All Tokens for User
    //                                  - Throw Error: Force Re-Login
    //                                Else:
    //                                  - Mark RT1: IsUsed=true, RevokedAt=Now
    //                                  - Create New Access Token
    //                                  - Create New RT2, Store: {Token: RT2, IsUsed: false}
    //                                  - Set RT1.ReplacedBy = RT2.Id (chuỗi trace)
    // <------------ New Access + RT2

    // (If Attacker uses RT1 again)
    // 3. Refresh Request (RT1) ----> Find RT1: IsUsed=true -> Detect Reuse
    //                                Revoke All Tokens for User
    //                                Send Alert Email
    // <------------ Error: Re-Login Required
    public async Task<AuthResponseDto> Login(
        AuthLoginDto login,
        string ipAddress,
        string? deviceInfo
    )
    {
        var user = await _userRepository
            .Query()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == login.Email);

        if (user == null)
        {
            throw new NotFoundException("Không tìm thấy người dùng với email đã cung cấp.");
        }

        if (!PasswordHelper.VerifyPassword(login.Password, user.PasswordHash))
        {
            throw new BadRequestException("Mật khẩu không đúng.");
        }

        if (!(user.IsActive ?? false))
        {
            throw new ForbiddenException("Tài khoản của bạn đã bị vô hiệu hóa.");
        }

        var accessToken = _jwtAuthService.GenerateToken(user);

        var refreshToken = await CreateRefreshTokenAsync(user, ipAddress, deviceInfo);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _accessTokenExpiryMinutes * 60,
        };
    }

    public async Task<string> CreateRefreshTokenAsync(
        User user,
        string ipAddress,
        string? deviceInfo = null
    )
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = await GenerateUniqueRefreshTokenAsync(),
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            DeviceInfo = deviceInfo,
            IsUsed = false,
            ReplacedByTokenId = null,
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshToken.Token;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        string? deviceInfo = null
    )
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            //tìm refresj token hiện tại
            var currentToken = await _refreshTokenRepository
                .Query()
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.ExpiresAt > DateTime.UtcNow);

            if (currentToken == null)
                throw new UnauthorizedAccessException("Refresh token không hợp lệ");

            //kiểm tra sử dụng token cũ
            if (currentToken.IsUsed || currentToken.RevokedAt != null)
            {
                currentToken.IsUsed = true;

                //thu hồi toàn bộ token khi phát hiện có 1 ai cố dùng token cũ
                await RevokeAllTokensForUserAsync(currentToken.UserId, ipAddress);

                await _unitOfWork.SaveChangesAsync();

                await SendSecurityAlertEmailAsync(currentToken.UserId, ipAddress, deviceInfo);
                await transaction.CommitAsync();

                throw new UnauthorizedAccessException(
                    "Phát hiện sử dụng lại refresh token đã bị thu hồi. Tất cả phiên đăng nhập đã bị buộc thoát. Vui lòng đăng nhập lại."
                );
            }

            //lấy user và validate
            var user = await _userRepository
                .Query()
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == currentToken.UserId);

            if (user == null || !(user.IsActive ?? false))
                throw new ForbiddenException("Tài khoản của bạn đã bị vô hiệu hóa");

            //đánh dấu token cũ
            currentToken.IsUsed = true;
            currentToken.RevokedAt = DateTime.UtcNow;
            currentToken.RevokedByIp = ipAddress;
            currentToken.LastUsedAt = DateTime.UtcNow;
            currentToken.LastUsedIp = ipAddress;

            //tạo access token mới
            var newAccessToken = _jwtAuthService.GenerateToken(user);

            //tạo refresh token
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = await GenerateUniqueRefreshTokenAsync(),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                DeviceInfo = deviceInfo,
                IsUsed = false,
            };

            //liên kết rotation old-> new
            currentToken.ReplacedByTokenId = newRefreshToken.Id;

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = _accessTokenExpiryMinutes * 60,
            };
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch { }
            throw;
        }
    }

    public async Task RevokeAllTokensForUserAsync(Guid userId, string ipAddress)
    {
        var tokens = await _refreshTokenRepository
            .Query()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.IsUsed = true;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendSecurityAlertEmailAsync(
        Guid userId,
        string ipAddress,
        string? deviceInfo = null
    )
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var member = await _memberRepository.Query().FirstOrDefaultAsync(m => m.UserId == userId);

        if (user != null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "FullName", user.FullName ?? "Hội viên" },
                { "MemberCode", member?.MemberCode ?? "N/A" },
                { "DetectionTime", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " (UTC)" },
                { "IpAddress", string.IsNullOrEmpty(ipAddress) ? "Không xác định" : ipAddress },
                { "DeviceInfo", string.IsNullOrEmpty(deviceInfo) ? "Không xác định" : deviceInfo },
                { "CurrentYear", DateTime.UtcNow.Year.ToString() },
                { "AppName", "Gym Management" },
            };
            await _emailService.SendTemplateAsync(
                templateCode: "SECURITY_ALERT",
                tenantId: user.TenantId,
                parameters: parameters,
                toEmail: user.Email
            );
        }
    }

    public async Task<string> Register(AuthRegisterDto register)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Kiểm tra email trùng
            var emailExists = await _userRepository
                .Query()
                .AnyAsync(u => u.Email == register.Email);

            if (emailExists)
                throw new BadRequestException("Email đã được sử dụng.");

            //kiểm tra sdt trùng
            var PhoneHash = PasswordHelper.HashPassword(register.Phone);

            var phoneExists = await _memberProfileRepository
                .Query()
                .AnyAsync(p => p.PhoneHash == PhoneHash);

            if (phoneExists)
                throw new BadRequestException("Số điện thoại đã được sử dụng.");

            // 2. Kiểm tra chi nhánh
            var branch =
                await _branchRepository.GetByIdAsync(register.HomeBranchId)
                ?? throw new BadRequestException("Chi nhánh không tồn tại.");

            // 3. Tạo Member
            var member = new Member
            {
                Id = Guid.NewGuid(),
                TenantId = branch.TenantId,
                HomeBranchId = register.HomeBranchId,
                MemberCode = await GenerateMemberCodeAsync(),
                Status = StatusType.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _memberRepository.AddAsync(member);

            // 4. Tạo MemberProfile (mã hóa)
            var profile = new MemberProfile
            {
                MemberId = member.Id,
                FullNameEnc = _encryptionService.Encrypt(register.FullName),
                PhoneEnc = _encryptionService.Encrypt(register.Phone),
                EmailEnc = _encryptionService.Encrypt(register.Email),
                PhoneHash = PasswordHelper.HashPassword(register.Phone),
                DateOfBirth = register.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(register.DateOfBirth.Value)
                    : null,
                Gender = register.Gender.ToString(),
                EmailVerified = EmailVerified.Unverified,
                AddressFullEnc = _encryptionService.Encrypt(register.Address.Full),
                WardEnc = _encryptionService.Encrypt(register.Address.Ward),
                DistrictEnc = _encryptionService.Encrypt(register.Address.District),
                ProvinceEnc = _encryptionService.Encrypt(register.Address.Province),
                DistrictHash = PasswordHelper.HashPassword(register.Address.District),
                ProvinceHash = PasswordHelper.HashPassword(register.Address.Province),
            };
            await _memberProfileRepository.AddAsync(profile);

            // 5. Tạo User (tài khoản đăng nhập)
            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = branch.TenantId,
                Email = register.Email,
                PasswordHash = PasswordHelper.HashPassword(register.Password),
                FullName = register.FullName,
                IsActive = IsActive.Inactive,
                EmailVerified = EmailVerified.Unverified,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Roles = new List<Role>(),
            };
            await _userRepository.AddAsync(user);

            // 6. Liên kết Member ↔ User
            member.UserId = user.Id;

            // 7. Gán role MEMBER
            var memberRole = await _roleRepository
                .Query()
                .FirstOrDefaultAsync(r => r.Code == "MEMBER" && r.TenantId == branch.TenantId);

            if (memberRole == null)
                throw new BadRequestException("Role hội viên chưa được cấu hình trong hệ thống.");

            user.Roles.Add(memberRole);

            // 9. Lưu tất cả
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            // 10. Gửi OTP (bắt buộc phải mở)
            await _verificationService.SendOtpAsync(
                email: register.Email,
                tenantId: branch.TenantId,
                purpose: "signup",
                userId: user.Id,
                memberId: member.Id,
                templateCode: "REGISTRATION_OTP",
                extraParameters: new Dictionary<string, string>
                {
                    { "FullName", register.FullName },
                    { "MemberCode", member.MemberCode },
                    { "BranchName", branch.Name },
                }
            );

            return member.MemberCode; // Có thể trả về mã hội viên để thông báo
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        // 1. Verify OTP và lấy kết quả
        var result = await _verificationService.VerifyOtpAsync(
            email: verifyEmailDto.Email,
            otp: verifyEmailDto.OTP,
            purpose: "signup"
        );

        // Nếu không throw → OTP đúng

        // 2. Kiểm tra purpose đúng là signup
        if (result.Purpose != "signup")
        {
            throw new BadRequestException("Mã OTP này không dùng để xác thực đăng ký.");
        }

        // 3. Lấy các entity cần cập nhật
        var user =
            await _userRepository.GetByIdAsync(result.UserId)
            ?? throw new BadRequestException("Không tìm thấy tài khoản.");

        var member = result.MemberId.HasValue
            ? await _memberRepository.GetByIdAsync(result.MemberId.Value)
            : null;

        var profile =
            member != null
                ? await _memberProfileRepository
                    .Query()
                    .FirstOrDefaultAsync(p => p.MemberId == member.Id)
                : null;

        var branch =
            member?.HomeBranchId != null
                ? await _branchRepository.GetByIdAsync(member.HomeBranchId.Value)
                : null;

        // 4. Kích hoạt tài khoản
        user.IsActive = IsActive.Active;
        user.EmailVerified = EmailVerified.Verified;
        user.EmailVerifiedAt = DateTime.UtcNow;

        if (profile != null)
        {
            profile.EmailVerified = EmailVerified.Verified;
            profile.EmailVerifiedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();

        // 5. Gửi email chào mừng
        var parameters = new Dictionary<string, string>
        {
            { "FullName", user.FullName ?? "Hội viên" },
            { "MemberCode", member?.MemberCode ?? "" },
            { "BranchName", branch?.Name ?? "Gym" },
            {
                "JoinDate",
                user.CreatedAt.HasValue ? user.CreatedAt.Value.ToString("dd/MM/yyyy") : ""
            },
            { "AppName", "Gym" },
            { "CurrentYear", DateTime.UtcNow.Year.ToString() },
        };

        try
        {
            await _emailService.SendTemplateAsync(
                templateCode: "WELCOME_NEW_MEMBER",
                tenantId: user.TenantId,
                parameters: parameters,
                toEmail: user.Email
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Gửi email thất bại.", ex);
        }

        return true;
    }

    private async Task<string> GenerateMemberCodeAsync()
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var lastMember = await _memberRepository
            .Query()
            .Where(m => m.MemberCode.StartsWith($"MEM-{yearMonth}"))
            .OrderByDescending(m => m.MemberCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastMember != null && int.TryParse(lastMember.MemberCode[10..], out int lastNumber))
            nextNumber = lastNumber + 1;

        return $"MEM-{yearMonth}-{nextNumber:D5}";
    }

    private async Task<string> GenerateUniqueRefreshTokenAsync()
    {
        string token;
        do
        {
            token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128));
        } while (await _refreshTokenRepository.Query().AnyAsync(t => t.Token == token));

        return token;
    }

    public async Task ChangePasswordAsync(Guid userId, RequestChangePasswordDto dto)
    {
        var user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Không tìm thấy người dùng.");

        if (!PasswordHelper.VerifyPassword(dto.OldPassword, user.PasswordHash))
            throw new BadRequestException("Mật khẩu cũ không đúng.");

        if (dto.NewPassword != dto.ConfirmPassword)
            throw new BadRequestException("Mật khẩu mới không khớp.");

        if (dto.NewPassword == dto.OldPassword)
        {
            throw new BadRequestException("Mật khẩu mới không được trùng với mật khẩu cũ.");
        }

        var member = await _memberRepository.Query().FirstOrDefaultAsync(m => m.UserId == userId);

        var parameters = new Dictionary<string, string>
        {
            { "FullName", user.FullName ?? "Hội viên" },
            { "MemberCode", member?.MemberCode ?? "N/A" },
            { "ChangeTime", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " (UTC)" },
            { "IpAddress", "Từ thiết bị đã đăng nhập" },
            { "CurrentYear", DateTime.UtcNow.Year.ToString() },
            { "AppName", "Gym Management" },
        };

        await _verificationService.SendOtpAsync(
            email: user.Email,
            tenantId: user.TenantId,
            purpose: "change_password",
            userId: userId,
            memberId: member?.Id,
            templateCode: "COMMON_OTP_VERIFICATION",
            extraParameters: parameters
        );
    }

    public async Task ConfirmChangePasswordAsync(Guid userId, ConfirmChangePasswordDto dto)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            var result = await _verificationService.VerifyOtpAsync(
                user.Email,
                dto.OTP,
                purpose: "change_password"
            );

            if (result.Purpose != "change_password")
                throw new BadRequestException("Mã OTP không hợp lệ cho mục đích đổi mật khẩu.");

            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            //vô hiệu hóa tất cả refresh tork -> logout trên mọi thiết bị
            await RevokeAllTokensForUserAsync(user.Id, "password_change_by_user");

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            var member = await _memberRepository
                .Query()
                .FirstOrDefaultAsync(m => m.UserId == user.Id);
            var parameters = new Dictionary<string, string>
            {
                { "FullName", user.FullName ?? "Hội viên" },
                { "MemberCode", member?.MemberCode ?? "N/A" },
                { "ChangeTime", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") + " (UTC)" },
                { "CurrentYear", DateTime.UtcNow.Year.ToString() },
                { "AppName", "Gym Management" },
                { "OTP", "THÀNH CÔNG" }, // để dùng chung template
                { "ActionDescription", "đổi mật khẩu thành công" },
            };

            await _emailService.SendTemplateAsync(
                templateCode: "PASSWORD_CHANGED_SUCCESS",
                tenantId: user.TenantId,
                parameters: parameters,
                toEmail: user.Email
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
