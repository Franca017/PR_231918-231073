using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IReviewRepository
    {
        IEnumerable<Review> GetAll();
        void Add(Review review);

    }
}
