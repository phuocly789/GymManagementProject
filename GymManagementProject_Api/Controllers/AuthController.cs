using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

//using GymManagementProject_Api.Models;

namespace GymManagementProject_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public async Task<string> Login([FromBody] AuthLoginDto login)
        {
            var result = await _authService.Login(login);

            return "{ \"access_token\": \"" + result + "\" }";
        }
    }
}
