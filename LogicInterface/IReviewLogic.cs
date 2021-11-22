using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IReviewLogic
    {
        void Add(Review newReview);
        void AdjustRating(int gameId, string userLoggedUserName);
        List<Review> GetGameReviews(int gameId);
    }
}