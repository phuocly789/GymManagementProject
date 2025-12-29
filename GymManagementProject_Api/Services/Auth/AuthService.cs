using dotnet03WebApi_EbayProject.Helper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public interface IAuthService
{
    Task<string> Login(AuthLoginDto login);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtAuthService _jwtAuthService;

    public AuthService(IUserRepository repository, JwtAuthService jwtAuthService)
    {
        _jwtAuthService = jwtAuthService;
        _userRepository = repository;
    }

    public async Task<string> Login(AuthLoginDto login)
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

        var token = _jwtAuthService.GenerateToken(user);

        return token;
    }
}
