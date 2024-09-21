using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Services;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMovieList(int page = 1)
        {
            var movies = await _movieService.GetMovieListAsync(page);
            return Ok(movies);
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetMovieDetails(int movieId)
        {
            var movieDetails = await _movieService.GetMovieDetailsAsync(movieId.ToString());

            if (movieDetails == null)
            {
                return NotFound();
            }

            return Ok(movieDetails);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string query, int page = 1)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required.");
            }

            var movies = await _movieService.SearchMoviesAsync(query, page);
            return Ok(movies);
        }
    }
}
