using System;

namespace Domain
{
    public class Log
    {
        public int Id { get; set; }
        public string Game { get; set; }
        public string User { get; set; } // Aca usamos string y no User porque el nombre de usuario es unico y es mas facil de manejar
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }

        public Log(string game, string user, DateTime date, string action, string message)
        {
            Game = game;
            User = user;
            Date = date;
            Action = action;
            Message = message;
        }
    }
}