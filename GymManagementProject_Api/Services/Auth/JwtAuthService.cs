using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManagementProject_Infrastructure.Data;
using GymManagementProject_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtAuthService
{
    private readonly string? _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly GymDbContext _context;
    private readonly int _accessTokenExpiryMinutes;

    public JwtAuthService(IConfiguration Configuration, GymDbContext db)
    {
        _key = Configuration["JwtConfigs:SecretKey"];
        _issuer = Configuration["JwtConfigs:Issuer"];
        _audience = Configuration["JwtConfigs:Audience"];
        _accessTokenExpiryMinutes = int.Parse(
            Configuration["JwtConfigs:AccessTokenExpiryMinutes"] ?? "60"
        );
        _context = db;
    }

    public string GenerateToken(User userLogin)
    {
        // Khóa bí mật để ký token
        var key = Encoding.UTF8.GetBytes(_key);
        // Tạo danh sách các claims cho token
        var claims = new List<Claim>
        {
            new Claim("Id", userLogin.Id.ToString()),
            new Claim("email", userLogin.Email), // Claim mặc định cho username
            new Claim("fullname", userLogin.FullName), // Claim tùy chỉnh cho full name
            new Claim(JwtRegisteredClaimNames.Sub, userLogin.FullName), // Subject của token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID của token
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), // Thời gian tạo token
        };

        var permissions = new HashSet<string>();
        // Lấy roles từ userLogin đã có (navigation property)
        if (userLogin.Roles != null && userLogin.Roles.Any())
        {
            foreach (var role in userLogin.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                if (role.Permissions != null && role.Permissions.Any())
                {
                    foreach (var permission in role.Permissions)
                    {
                        // Tránh thêm quyền trùng lặp
                        if (permissions.Add(permission.Name))
                        {
                            claims.Add(new Claim("permission", permission.Name));
                        }
                    }
                }
            }
        }

        // Tạo khóa bí mật để ký token
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );
        // Thiết lập thông tin cho token

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            SigningCredentials = credentials,   
            Issuer = _issuer, // Thêm Issuer vào token
            Audience = _audience,
        };
        // Tạo token bằng JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Trả về chuỗi token đã mã hóa
        return tokenHandler.WriteToken(token);
    }

    public string DecodePayloadToken(string token)
    {
        try
        {
            // Kiểm tra token có null hoặc rỗng không
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token không được để trống", nameof(token));
            }

            // Tạo handler và đọc token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Lấy username từ claims (thường nằm trong claim "sub" hoặc "name")
            var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "username"); // Common in some identity providers

            if (usernameClaim == null)
            {
                throw new InvalidOperationException("Không tìm thấy username trong payload");
            }

            return usernameClaim.Value;
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (có thể log lỗi ở đây)
            throw new InvalidOperationException($"Lỗi khi decode token: {ex.Message}", ex);
        }
    }
}
