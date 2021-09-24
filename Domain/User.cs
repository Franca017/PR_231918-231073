using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public List<Game> PurchasedGames { get; set; }

        public User(string userName, DateTime date)
        {
            this.UserName = userName;
            this.DateCreated = date;
            this.PurchasedGames = new List<Game>();
        }

    }
}
