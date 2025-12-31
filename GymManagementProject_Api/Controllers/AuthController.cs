using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

//using GymManagementProject_Api.Models;

namespace GymManagementProject_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [EnableRateLimiting("IpPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Login")]
        public async Task<ResponseValue<AuthResponseDto>> Login([FromBody] AuthLoginDto login)
        {
            var result = await _authService.Login(login, GetIpAddress(), GetDeviceInfo());

            return new ResponseValue<AuthResponseDto>(
                result,
                "Đăng nhập thành công.",
                StatusReponse.Success
            );
        }

        [HttpPost("Register")]
        public async Task<ResponseValue<string>> Register([FromBody] AuthRegisterDto dto)
        {
            try
            {
                // 2. Gọi service xử lý đăng ký
                var memberCode = await _authService.Register(dto);

                // 3. Trả về thành công + mã hội viên để hiển thị cho người dùng
                var successMessage =
                    "Đăng ký thành công! Chúng tôi đã gửi mã OTP đến email của bạn. Vui lòng kiểm tra hộp thư (và thư rác) để xác thực tài khoản.";

                return new ResponseValue<string>(memberCode, successMessage, StatusReponse.Success);
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                // Lỗi bất ngờ → log lại để debug
                throw new Exception("Đã có lỗi xảy ra trong quá trình đăng ký.", ex);
            }
        }

        [HttpPost("VerifyEmail")]
        public async Task<ResponseValue<bool>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                throw new BadRequestException(string.Join("; ", errors));
            }

            try
            {
                await _authService.VerifyEmailAsync(verifyEmailDto);

                return new ResponseValue<bool>(
                    true,
                    "Xác thực email thành công! 🎉. Bạn có thể đăng nhập ngay bây giờ bằng email và mật khẩu đã đăng ký.",
                    StatusReponse.Success
                );
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Đã có lỗi xảy ra trong quá trình xác thực email.", ex);
            }
        }

        [HttpPost("RefreshToken")]
        [Authorize]
        public async Task<ResponseValue<AuthResponseDto>> RefreshToken(
            [FromBody] RefreshTokenRequestDto refreshTokenRequestDto
        )
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Refresh token không được để trống.");

            var result = await _authService.RefreshTokenAsync(
                refreshTokenRequestDto.RefreshToken,
                GetIpAddress(),
                GetDeviceInfo()
            );
            return new ResponseValue<AuthResponseDto>(
                result,
                "Refresh token thành công.",
                StatusReponse.Success
            );
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ResponseValue<bool>> ChangePassword(
            [FromBody] RequestChangePasswordDto dto
        )
        {
            //lấy user từ token
            var userIdClaim =
                User.FindFirst("Id")?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác định người dùng");
            var userId = Guid.Parse(userIdClaim);

            await _authService.ChangePasswordAsync(userId, dto);

            return new ResponseValue<bool>(
                true,
                "Gửi yêu cầu thành công, Vui lòng kiểm tra Email.",
                StatusReponse.Success
            );
        }

        [HttpPost("ConfirmChangePassword")]
        [Authorize]
        public async Task<ResponseValue<bool>> ConfirmChangePassword(
            [FromBody] ConfirmChangePasswordDto dto
        )
        {
            var userIdClaim =
                User.FindFirst("Id")?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác định người dùng");
            var userId = Guid.Parse(userIdClaim);
            await _authService.ConfirmChangePasswordAsync(userId,dto);

            return new ResponseValue<bool>(true, "Đổi mật khẩu thành công.", StatusReponse.Success);
        }

        private string GetIpAddress()
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? "Unknown";
        }

        private string GetDeviceInfo()
        {
            return Request.Headers["User-Agent"].ToString();
        }
    }
}
