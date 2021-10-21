using System.Collections.Generic;
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
