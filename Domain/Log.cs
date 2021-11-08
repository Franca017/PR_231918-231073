using System;

namespace Domain
{
    public class Log
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string User { get; set; } // Aca usamos string y no User porque el nombre de usuario es unico y es mas facil de manejar
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }

        public Log(int gameId, string gameTitle, string user, DateTime date, string action, string message)
        {
            GameId = gameId;
            GameTitle = gameTitle;
            User = user;
            Date = date;
            Action = action;
            Message = message;
        }
    }
}