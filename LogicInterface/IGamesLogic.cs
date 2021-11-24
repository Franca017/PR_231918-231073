using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace LogicInterface
{
    public interface IGamesLogic
    {
        List<Game> GetAll();
        Game GetById(int gameId);
        Game Add(Game game);
        List<Game> GetSearchedGames(string keywords);
        string Delete(int gameId, string userLoggedUserName);
        List<Game> GetPublishedGames(User userLogged);
        string Modify(string[] modifySplit, string userLoggedUserName);
        void AdjustRating(int gameId, int newRating, string userLoggedUserName);
        List<Game> GetGamesOverRating(int minRating);
        void ModifyImage(string[] modifySplit, string userLoggedUserName);
    }
}