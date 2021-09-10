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
        private readonly List<Game> games;

        public GameRepository()
        {
            //this.contexto = contexto;
            this.games = new List<Game>();
        }

        public void Add(Game game)
        {
            throw new NotImplementedException();
        }

        public List<Game> GetAll()
        {
            return this.games;
        }
    }
}
