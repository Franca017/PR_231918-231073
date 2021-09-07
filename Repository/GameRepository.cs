using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;

namespace Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly DbContext contexto;
        private readonly DbSet<Game> games;

        public GameRepository(DbContext contexto)
        {
            this.contexto = contexto;
            this.games = contexto.Set<Game>();
        }

        public void Add(Game game)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Game> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
