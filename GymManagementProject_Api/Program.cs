using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using GymManagementProject_Api.Middlewares;
using GymManagementProject_Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// connect to database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing in appsettings.Development.json"
    );

builder.Services.AddDbContext<GymDbContext>(options => options.UseNpgsql(connectionString));

//Đăng ký repository service
builder.Services.AddRepositoryServices();

//cache
builder.Services.AddMemoryCache();

//controllers
builder
    .Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context
                .ModelState.Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var errorResponse = new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Error = "Bad Request",
                Message = string.Join("; ", errors),
            };

            return new BadRequestObjectResult(errorResponse);
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//Fluent Validation
builder.Services.AddValidatorsFromAssemblyContaining<AuthLoginDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AuthRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddressDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VerifyEmailDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gym Management API", Version = "v1" });

    // Định nghĩa security scheme cho Bearer token
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập token vào ô bên dưới theo định dạng: Bearer {token}",
        }
    );

    // Áp dụng security requirement toàn cục (mọi API đều cần token)
    // Cách mới: dùng lambda để access OpenApiDocument nếu cần (ở đây đơn giản nên không cần)
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        builder =>
            builder
                .WithOrigins("http://localhost:5131")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    );
});

// JWt
var privateKey = builder.Configuration["JwtConfigs:SecretKey"];
var Issuer = builder.Configuration["JwtConfigs:Issuer"];
var Audience = builder.Configuration["JwtConfigs:Audience"];

if (
    string.IsNullOrWhiteSpace(privateKey)
    || string.IsNullOrWhiteSpace(Issuer)
    || string.IsNullOrWhiteSpace(Audience)
)
{
    throw new InvalidOperationException(
        "JWT configuration missing: JWT_SECRET_KEY, JWT_ISSUER, or JWT_AUDIENCE is not set."
    );
}

// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Thiết lập các tham số xác thực token
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
            ValidateIssuer = true,
            ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
            // Kiểm tra và xác nhận Audience (đối tượng nhận token)
            ValidateAudience = true,
            ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
            // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
            // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
            // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
            ClockSkew = TimeSpan.Zero,
            // Xác định claim chứa vai trò của user (để phân quyền)
            RoleClaimType = ClaimTypes.Role,
            // Xác định claim chứa tên của user
            NameClaimType = ClaimTypes.Name,
            // Kiểm tra thời gian hết hạn của token, không cho phépa sử dụng token hết hạn
            ValidateLifetime = true,
        };
    });

//email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("SmtpConfigs"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Encryption service
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// Di service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

//add jwt service
builder.Services.AddScoped<JwtAuthService>();

//dùng permission từ database để tạo policy
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

//middleware chống spam
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    //limit
    options.AddFixedWindowLimiter(
        "IpPolicy",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(10);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        }
    );

    options.OnRejected = async (context, token) =>
    {
        var http = context.HttpContext;

        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        http.Response.ContentType = "application/json";

        var error = new ErrorResponse
        {
            StatusCode = StatusCodes.Status429TooManyRequests,
            Error = "Too Many Requests",
            Message = "Quá nhiều yêu cầu. Vui lòng thử lại sau 10 phút.",
        };

        await http.Response.WriteAsync(JsonSerializer.Serialize(error), token);
    };
});
var app = builder.Build();

//sử dụng middle chống spam
app.UseRateLimiter();

//middleware xử lý lỗi toàn cục
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
