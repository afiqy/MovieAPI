using Microsoft.AspNetCore.Mvc;
using MovieAPI.Data;
using MovieAPI.DTOs;
using MovieAPI.Models;
using MovieAPI.Services;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ICognitoService _cognitoService;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(ICognitoService cognitoService, ApplicationDbContext dbContext)
        {
            _cognitoService = cognitoService;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var result = await _cognitoService.RegisterUserAsync(dto.Email, dto.Password);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            var user = new User
            {
                Email = dto.Email,
                CognitoUserId = result.CognitoUserId  // Store the Cognito User ID locally
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("User registered and confirmed successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var (idToken, accessToken) = await _cognitoService.LoginAsync(dto.Email, dto.Password);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Failed to get access token.");
                }

                return Ok(new { AccessToken = accessToken });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
