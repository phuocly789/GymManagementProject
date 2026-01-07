using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymManagementProject_Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//using GymManagementProject_Api.Models;

namespace GymManagementProject_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetAllUsers")]
        [Authorize(Policy = "users:view")]
        public async Task<PagedResult<UserResponseDto>> GetAllUsers()
        {
            return await _userService.GetAllUsersAsync();
        }

        [HttpGet("GetUserById/{id}")]
        [Authorize(Policy = "users:view")]
        public async Task<UserDetailDto> GetUserById(Guid id)
        {
            var userIdClaim =
                User.FindFirst("Id")?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác định người dùng");
            var currentUserId = Guid.Parse(userIdClaim);

            return await _userService.GetUserByIdAsync(currentUserId, id);
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task DeleteUser(Guid id)
        {
            await _userService.DeleteAsync(id);
        }

        // [HttpGet("{id}")]
        // public async Task<ActionResult<TModel>> GetTModelById(int id)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return null;
        // }

        // [HttpPost("")]
        // public async Task<ActionResult<TModel>> PostTModel(TModel model)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return null;
        // }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutTModel(int id, TModel model)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return NoContent();
        // }

        // [HttpDelete("{id}")]
        // public async Task<ActionResult<TModel>> DeleteTModelById(int id)
        // {
        //     // TODO: Your code here
        //     await Task.Yield();

        //     return null;
        // }
    }
}
