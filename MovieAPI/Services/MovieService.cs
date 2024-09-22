using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using MovieAPI.DTOs;
using MovieAPI.Models;
using Microsoft.Extensions.Logging;
using MovieAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MovieAPI.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;
        private readonly RedisCacheService _redisCacheService;
        private readonly string _apiKey;
        private readonly ILogger<MovieService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private const int PageSize = 10;

        public MovieService(HttpClient httpClient, RedisCacheService redisCacheService, IConfiguration config, ILogger<MovieService> logger, ApplicationDbContext dbContext)
        {
            _httpClient = httpClient;
            _redisCacheService = redisCacheService;
            _apiKey = config["TmdbApi:ApiKey"];
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<MovieListResponse> GetMovieListAsync(int page)
        {
            string cacheKey = $"MovieList_Page_{page}";
            try
            {
                var cachedMovies = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedMovies))
                {
                    return JsonSerializer.Deserialize<MovieListResponse>(cachedMovies);
                }

                var response = await _httpClient.GetAsync("https://api.themoviedb.org/3/movie/popular?api_key={_apiKey}&page={page}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching movie list. Status Code: {response.StatusCode}");
                    return null;
                }

                var movieList = await response.Content.ReadAsStringAsync();
                var movieApiResponse = JsonSerializer.Deserialize<MovieListResponse>(movieList);

                foreach (var movieDto in movieApiResponse.Results)
                {
                    var newDto = new MovieDto
                    {
                        ExternalId = movieDto.Id.ToString(),
                        Title = movieDto.Title,
                        Overview = movieDto.Overview,
                        Poster_Path = movieDto.Poster_Path,
                        Release_Date = movieDto.Release_Date,
                        Adult = movieDto.Adult,
                        Vote_Average = movieDto.Vote_Average,
                        Vote_Count = movieDto.Vote_Count
                    };

                    await SaveOrUpdateMovieAsync(newDto);
                }

                await _redisCacheService.CacheMovieAsync(cacheKey, movieList, TimeSpan.FromHours(1));

                return movieApiResponse;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return null;
            }
        }

        public async Task<MovieDto> GetMovieDetailsAsync(string movieId)
        {
            string cacheKey = $"MovieDetails_{movieId}";
            try
            {
                var cachedMovie = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedMovie))
                {
                    return JsonSerializer.Deserialize<MovieDto>(cachedMovie);
                }

                var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/movie/{movieId}?api_key={_apiKey}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching movie details for {movieId}. Status Code: {response.StatusCode}");
                    return null;
                }

                var movieDetailsJson = await response.Content.ReadAsStringAsync();
                var movieDetails = JsonSerializer.Deserialize<MovieDto>(movieDetailsJson);

                await SaveOrUpdateMovieAsync(movieDetails);

                await _redisCacheService.CacheMovieAsync(cacheKey, movieDetailsJson, TimeSpan.FromHours(1));

                return movieDetails;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return null;
            }
        }

        public async Task<MovieListResponse> SearchMoviesAsync(string query, int page)
        {
            string cacheKey = $"MovieSearch_{query}_Page_{page}";
            try
            {
                var cachedSearchResults = await _redisCacheService.GetCachedMovieAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedSearchResults))
                {
                    return JsonSerializer.Deserialize<MovieListResponse>(cachedSearchResults);
                }

                var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={query}&page={page}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error searching for movies with query '{query}'. Status Code: {response.StatusCode}");
                    return null;
                }

                var searchResults = await response.Content.ReadAsStringAsync();
                var movieApiResponse = JsonSerializer.Deserialize<MovieListResponse>(searchResults);

                foreach (var movieDto in movieApiResponse.Results)
                {
                    var newDto = new MovieDto
                    {
                        ExternalId = movieDto.Id.ToString(),
                        Title = movieDto.Title,
                        Overview = movieDto.Overview,
                        Poster_Path = movieDto.Poster_Path,
                        Release_Date = movieDto.Release_Date,
                        Adult = movieDto.Adult,
                        Vote_Average = movieDto.Vote_Average,
                        Vote_Count = movieDto.Vote_Count
                    };

                    await SaveOrUpdateMovieAsync(newDto);
                }


                await _redisCacheService.CacheMovieAsync(cacheKey, searchResults, TimeSpan.FromHours(1));

                return movieApiResponse;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Error: {ex.Message}");
                return null;
            }
        }

        private async Task SaveOrUpdateMovieAsync(MovieDto movieDto)
        {
            var existingMovie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.ExternalId == movieDto.ExternalId);

            if (existingMovie == null)
            {
                var newMovie = MapDtoToModel(movieDto);
                _dbContext.Movies.Add(newMovie);
            }
            else
            {
                existingMovie.Title = movieDto.Title;
                existingMovie.Description = movieDto.Overview;
            }

            await _dbContext.SaveChangesAsync();
        }

        private Movie MapDtoToModel(MovieDto movieDto)
        {
            return new Movie
            {
                ExternalId = movieDto.ExternalId,
                Title = movieDto.Title,
                Description = movieDto.Overview
            };
        }
    }
}
