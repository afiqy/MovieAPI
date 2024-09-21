namespace MovieAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public ICollection<Favourite> Favourites { get; set; }
    }
}
