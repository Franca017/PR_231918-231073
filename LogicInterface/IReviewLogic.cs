using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IReviewLogic
    {
        void Add(Review newReview);
        void AdjustRating(int gameId);
        List<Review> GetGameReviews(int gameId);
    }
}