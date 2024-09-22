using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace MovieAPI.Controllers
{
    [Route("api/favourites")]
    [ApiController]
    [Authorize]
    public class FavouritesController : ControllerBase
    {
        private readonly FavouriteService _favouriteService;

        public FavouritesController(FavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFavourites(int page = 1)
        {
            var cognitoUserId = GetCognitoUserIdFromToken();
            if (cognitoUserId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var favourites = await _favouriteService.GetFavouritesForUserAsync(cognitoUserId, page);
            return Ok(favourites);
        }

        [HttpPost("{movieId}")]
        public async Task<IActionResult> AddToFavourites(int movieId)
        {
            var cognitoUserId = GetCognitoUserIdFromToken();
            if (cognitoUserId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var result = await _favouriteService.AddToFavouritesAsync(cognitoUserId, movieId);

            if (!result)
            {
                return BadRequest("Failed to add to favourites.");
            }

            return Ok("Added to favourites.");
        }

        [HttpDelete("{movieId}")]
        public async Task<IActionResult> RemoveFromFavourites(int movieId)
        {
            var cognitoUserId = GetCognitoUserIdFromToken();
            if (cognitoUserId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var result = await _favouriteService.RemoveFromFavouritesAsync(cognitoUserId, movieId);

            if (!result)
            {
                return BadRequest("Failed to remove from favourites.");
            }

            return Ok("Removed from favourites.");
        }

        private string GetCognitoUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            return userIdClaim;
        }
    }
}
