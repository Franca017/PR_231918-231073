using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using RepositoryInterface;

namespace Repository
{
    public class GameRepository : IGameRepository
    {
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
