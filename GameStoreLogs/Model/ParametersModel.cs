
namespace GameStoreLogs.Model
{
    public class ParametersModel
    {
        public string GameTitle { get; set; }
        public string UserName { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Date { get; set; }

        public ParametersModel(string gameTitle, string userName, string dateFrom, string dateTo, string date)
        {
            GameTitle = gameTitle;
            UserName = userName;
            DateFrom = dateFrom;
            DateTo = dateTo;
            Date = date;
        }
    }
}