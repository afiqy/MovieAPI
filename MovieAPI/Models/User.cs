namespace MovieAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string CognitoUserId { get; set; }
        public ICollection<Favourite> Favourites { get; set; }
    }
}
