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
        private readonly List<Review> reviews;

        public ReviewRepository()
        {
            this.reviews = new List<Review>();
        }

        public void Add(Review review)
        {
            var highestId = reviews.Any() ? reviews.Max(x => x.Id) : 0;
            review.Id = highestId + 1;
            reviews.Add(review);
        }

        public IEnumerable<Review> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
