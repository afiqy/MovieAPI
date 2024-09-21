using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using MovieAPI.Models;
using Microsoft.Extensions.Logging;

namespace MovieAPI.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;
        private readonly RedisCacheService _redisCacheService;
        private readonly string _apiKey;
        private readonly ILogger<MovieService> _logger;
        private const int PageSize = 10;

        public MovieService(HttpClient httpClient, RedisCacheService redisCacheService, IConfiguration config, ILogger<MovieService> logger)
        {
            _httpClient = httpClient;
            _redisCacheService = redisCacheService;
            _apiKey = config["TmdbApi:ApiKey"];
            _logger = logger;
        }

        public async Task<object> GetMovieListAsync(int page)
        {
            string cacheKey = $"MovieList_Page_{page}";
            try
            {
                var cachedMovies = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedMovies))
                {
                    return JsonSerializer.Deserialize<object>(cachedMovies);
                }

                var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/movie/popular?api_key={_apiKey}&page={page}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching movie list. Status Code: {response.StatusCode}");
                    return HandleErrorResponse(response);
                }

                var movieList = await response.Content.ReadAsStringAsync();

                await _redisCacheService.CacheMovieAsync(cacheKey, movieList, TimeSpan.FromHours(1));

                return JsonSerializer.Deserialize<object>(movieList);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return "There was an error retrieving the movie list. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return "An unexpected error occurred. Please try again later.";
            }
        }

        public async Task<object> GetMovieDetailsAsync(string movieId)
        {
            string cacheKey = $"MovieDetails_{movieId}";
            try
            {
                var cachedMovie = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedMovie))
                {
                    return JsonSerializer.Deserialize<object>(cachedMovie);
                }

                var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/movie/{movieId}?api_key={_apiKey}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching movie details for {movieId}. Status Code: {response.StatusCode}");
                    return HandleErrorResponse(response);
                }

                var movieDetails = await response.Content.ReadAsStringAsync();

                await _redisCacheService.CacheMovieAsync(cacheKey, movieDetails, TimeSpan.FromHours(1));

                return JsonSerializer.Deserialize<object>(movieDetails);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return "There was an error retrieving the movie details. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return "An unexpected error occurred. Please try again later.";
            }
        }

        public async Task<object> SearchMoviesAsync(string query, int page)
        {
            string cacheKey = $"MovieSearch_{query}_Page_{page}";
            try
            {
                var cachedSearchResults = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedSearchResults))
                {
                    return JsonSerializer.Deserialize<object>(cachedSearchResults);
                }

                var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={query}&page={page}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error searching for movies with query '{query}'. Status Code: {response.StatusCode}");
                    return HandleErrorResponse(response);
                }

                var searchResults = await response.Content.ReadAsStringAsync();

                await _redisCacheService.CacheMovieAsync(cacheKey, searchResults, TimeSpan.FromHours(1));

                return JsonSerializer.Deserialize<object>(searchResults);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return "There was an error searching for movies. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return "An unexpected error occurred. Please try again later.";
            }
        }

        private string HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return "The requested resource was not found.";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "Unauthorized request. Please check your API key.";
            }
            else
            {
                return $"An error occurred. Status code: {response.StatusCode}.";
            }
        }
    }
}
