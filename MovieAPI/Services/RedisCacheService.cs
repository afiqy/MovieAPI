using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace MovieAPI.Services
{
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> GetCachedMovieAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }

        public async Task CacheMovieAsync(string key, string value, TimeSpan expiration)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiration);
        }
    }
}
