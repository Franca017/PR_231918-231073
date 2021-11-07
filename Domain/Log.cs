using System;

namespace Domain
{
    public class Log
    {
        public Game Game { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }

        public Log()
        {
        }
    }
}