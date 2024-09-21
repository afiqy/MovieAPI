using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MovieAPI.Data;
using MovieAPI.Models;

namespace MovieAPI.Services
{
    public class FavouriteService
    {
        private readonly ApplicationDbContext _dbContext;

        public FavouriteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Movie>> GetFavouritesForUserAsync(int userId, int page)
        {
            return await _dbContext.Favourites
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Id)
                .Skip((page - 1) * 10)
                .Take(10)
                .Select(f => f.Movie)
                .ToListAsync();
        }

        public async Task<bool> AddToFavouritesAsync(int userId, int movieId)
        {
            var existingFavourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

            if (existingFavourite != null)
            {
                return false;
            }

            var favourite = new Favourite
            {
                UserId = userId,
                MovieId = movieId
            };

            _dbContext.Favourites.Add(favourite);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromFavouritesAsync(int userId, int movieId)
        {
            var favourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

            if (favourite == null)
            {
                return false;
            }

            _dbContext.Favourites.Remove(favourite);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
