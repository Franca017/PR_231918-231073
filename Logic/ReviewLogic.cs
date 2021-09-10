using System;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class ReviewLogic : IReviewLogic
    {
        private IReviewRepository reviewRepository;

        public ReviewLogic(IServiceProvider serviceProvider)
        {
            reviewRepository = serviceProvider.GetService<IReviewRepository>();
            
        }
    }
}