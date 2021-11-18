namespace GameStoreAdminServer.Models
{
    public class GameInModel
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Sinopsis { get; set; }

        public GameInModel(string title, string genre, string sinopsis)
        {
            Title = title;
            Genre = genre;
            Sinopsis = sinopsis;
        }
    }
}