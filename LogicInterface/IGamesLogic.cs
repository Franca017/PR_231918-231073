using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IGamesLogic
    {
        List<Game> GetAll();
        Game GetById(int gameId);
        Game Add(Game game);
    }
}