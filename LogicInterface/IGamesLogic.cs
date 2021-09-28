using System.Collections;
using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IGamesLogic
    {
        List<Game> GetAll();
        Game GetById(int gameId);
        Game Add(Game game);
        List<Game> GetSearchedGames(string keywords);
        void Delete(int gameId);
        List<Game> GetPublishedGames(User userLogged);
        void Modify(string[] modifySplit);
        void AdjustRating(int gameId, int newRating);
        List<Game> GetGamesOverRating(int minRating);
    }
}