using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IUserLogic
    {
        User Login(string userName);
        List<User> GetAll();
        User GetById(int userId);
        string PurchaseGame(User userLogged, int gameId);
        string SellGame(User user, int requestGameId);
        List<Game> GetPurchasedGames(int userLoggedId);
        User Add(User newUser);
        User Modify(int requestId, string requestName);
        void Delete(int requestId);
    }
}