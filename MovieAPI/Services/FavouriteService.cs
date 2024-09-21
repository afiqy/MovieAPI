using MovieAPI.Data;
using MovieAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPI.Services
{
    public class FavouriteService
    {
        private readonly ApplicationDbContext _dbContext;

        public FavouriteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Movie>> GetFavouritesForUserAsync(string cognitoUserId, int page)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);
            if (user == null) return null;

            return await _dbContext.Favourites
                .Include(f => f.Movie)
                .Where(f => f.UserId == user.Id)
                .OrderBy(f => f.Id)
                .Skip((page - 1) * 10)
                .Take(10)
                .Select(f => f.Movie)
                .ToListAsync();
        }

        public async Task<bool> AddToFavouritesAsync(string cognitoUserId, int movieId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);
            if (user == null) return false;

            var existingFavourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.MovieId == movieId);

            if (existingFavourite != null)
            {
                return false; // Movie is already in favourites
            }

            var favourite = new Favourite
            {
                UserId = user.Id,
                MovieId = movieId
            };

            _dbContext.Favourites.Add(favourite);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromFavouritesAsync(string cognitoUserId, int movieId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CognitoUserId == cognitoUserId);
            if (user == null) return false;

            var favourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.MovieId == movieId);

            if (favourite == null)
            {
                return false; // Favourite not found
            }

            _dbContext.Favourites.Remove(favourite);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
