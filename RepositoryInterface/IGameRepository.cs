using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IGameRepository
    {
        IEnumerable<Game> GetAll();
        void Add(Game game);


    }
}
