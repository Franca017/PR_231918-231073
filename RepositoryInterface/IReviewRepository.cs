using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IReviewRepository
    {
        List<Review> GetAll();
        void Add(Review review);

        List<Review> GetByGame(int gameId);
    }
}
