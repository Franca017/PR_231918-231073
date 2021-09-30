
namespace Domain
{
    public class Review
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Game Game { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public Review(User user, Game game, int rating, string comment)
        {
            this.User = user;
            this.Game = game;
            this.Rating = rating;
            this.Comment = comment;
        }
    }
}
