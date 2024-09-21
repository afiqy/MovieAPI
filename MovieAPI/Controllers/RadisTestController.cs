using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly RedisCacheService _redisCacheService;

        public TestController(RedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }

        [HttpGet("cache")]
        public async Task<IActionResult> TestCache()
        {
            string cacheKey = "TestKey";
            string cachedValue = await _redisCacheService.GetCachedMovieAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedValue))
            {
                string newValue = "This is a cached value.";
                await _redisCacheService.CacheMovieAsync(cacheKey, newValue, TimeSpan.FromMinutes(10));
                cachedValue = newValue;
            }

            return Ok(new { CachedValue = cachedValue });
        }

        [HttpGet("validate-jwt")]
        [Authorize]
        public IActionResult ValidateJwtToken()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return Ok(new
            {
                Message = "JWT Token is valid!",
                Claims = claims
            });
        }
    }
}
