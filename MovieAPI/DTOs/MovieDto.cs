namespace MovieAPI.DTOs
{
    public class MovieDto
    {
        public string ExternalId { get; set; } 
        public string Title { get; set; }
        public string? Overview { get; set; }
        public double? Popularity { get; set; }
        public string? Poster_Path { get; set; }
        public string? Release_Date { get; set; }
        public bool Adult { get; set; }
        public double Vote_Average { get; set; }
        public int Vote_Count { get; set; }
    }
}
