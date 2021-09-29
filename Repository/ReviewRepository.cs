using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;

namespace Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly List<Review> _reviews;
        private static readonly object Locker = new object();

        public ReviewRepository()
        {
            _reviews = new List<Review>();
        }

        public void Add(Review review)
        {
            lock (Locker)
            {
                var highestId = _reviews.Any() ? _reviews.Max(x => x.Id) : 0;
                review.Id = highestId + 1;
                _reviews.Add(review);
            }
        }

        public List<Review> GetByGame(int gameId)
        {
            lock (Locker)
            {
                return _reviews.FindAll(e => e.Game.Id.Equals(gameId));
            }
        }

        public List<Review> GetAll()
        {
            lock (Locker)
            {
                return _reviews;
            }
        }
    }
}
