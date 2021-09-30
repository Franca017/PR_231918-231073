using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IUserLogic
    {
        User Login(string userName);
        string PurchaseGame(User userLogged, int gameId);
        List<Game> GetPurchasedGames(int userLoggedId);
    }
}