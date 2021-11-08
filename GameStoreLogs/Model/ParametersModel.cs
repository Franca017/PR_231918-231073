
namespace GameStoreLogs.Model
{
    public class ParametersModel
    {
        public string GameTitle { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }

        public ParametersModel(string gameTitle, string userName, string date)
        {
            GameTitle = gameTitle;
            UserName = userName;
            Date = date;
        }
    }
}