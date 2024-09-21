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
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var favourites = await _favouriteService.GetFavouritesForUserAsync((int)userId, page);
            return Ok(favourites);
        }

        [HttpPost("{movieId}")]
        public async Task<IActionResult> AddToFavourites(int movieId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var result = await _favouriteService.AddToFavouritesAsync((int)userId, movieId);

            if (!result)
            {
                return BadRequest("Failed to add to favourites.");
            }

            return Ok("Added to favourites.");
        }

        [HttpDelete("{movieId}")]
        public async Task<IActionResult> RemoveFromFavourites(int movieId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            var result = await _favouriteService.RemoveFromFavouritesAsync((int)userId, movieId);

            if (!result)
            {
                return BadRequest("Failed to remove from favourites.");
            }

            return Ok("Removed from favourites.");
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }


    }
}
