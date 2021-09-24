using System;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class ReviewLogic : IReviewLogic
    {
        private IReviewRepository reviewRepository;
        private IGamesLogic _gamesLogic;

        public ReviewLogic(IServiceProvider serviceProvider)
        {
            reviewRepository = serviceProvider.GetService<IReviewRepository>();
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
        }

        public void Add(Review newReview)
        {
            this.reviewRepository.Add(newReview);
            this._gamesLogic.AddReviewToGame(newReview);
        }
    }
}