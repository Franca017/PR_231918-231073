using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IReviewInterface
    {
        IEnumerable<Review> GetAll();
        void Add(Review review);

    }
}
