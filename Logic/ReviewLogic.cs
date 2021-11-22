using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class ReviewLogic : IReviewLogic
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IGamesLogic _gamesLogic;

        public ReviewLogic(IServiceProvider serviceProvider)
        {
            _reviewRepository = serviceProvider.GetService<IReviewRepository>();
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
        }

        public void Add(Review newReview)
        {
            _reviewRepository.Add(newReview);
        }

        public void AdjustRating(int gameId, string userLoggedUserName)
        {
            var reviews = _reviewRepository.GetByGame(gameId);
            var sum = reviews.Sum(review => review.Rating);

            var newRating = sum / (reviews.Count);
            _gamesLogic.AdjustRating(gameId, newRating, userLoggedUserName);
        }

        public List<Review> GetGameReviews(int gameId)
        {
            return _reviewRepository.GetByGame(gameId);
        }
    }
}