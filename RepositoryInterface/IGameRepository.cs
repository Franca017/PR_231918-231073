using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IGameRepository
    {
        List<Game> GetAll();
        void Add(Game game);
        Game GetById(int gameId);
    }
}
