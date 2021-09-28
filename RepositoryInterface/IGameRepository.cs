using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IGameRepository
    {
        List<Game> GetAll();
        Game Add(Game game);
        Game GetById(int gameId);
        void Delete(int gameId);
        List<Game> GetPublishedGames(User userLogged);
        List<Game> GetGamesOverRating(int minRating);
    }
}
