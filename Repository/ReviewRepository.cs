using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;

namespace Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DbContext contexto;
        private readonly DbSet<Review> reviews;

        public ReviewRepository(DbContext contexto)
        {
            this.contexto = contexto;
            this.reviews = contexto.Set<Review>();
        }

        public void Add(Review review)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Review> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
