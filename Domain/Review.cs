using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class Review
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Game Game { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

    }
}
