using Domain;

namespace LogicInterface
{
    public interface IUserLogic
    {
        User Login(string userName);
        string PurchaseGame(User userLogged, int gameId);
    }
}