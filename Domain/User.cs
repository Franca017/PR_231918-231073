using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<Game> PublishedGames { get; set; }
        public List<Game> PurchasedGames { get; set; }

    }
}
