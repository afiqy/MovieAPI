namespace MovieAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public string Overview { get; internal set; }
        public string Poster_Path { get; internal set; }
        public string Release_Date { get; internal set; }
        public bool Adult { get; internal set; }
        public double Vote_Average { get; internal set; }
        public int Vote_Count { get; internal set; }

        public ICollection<Favourite> Favourites { get; set; }
    }
}
