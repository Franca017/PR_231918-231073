using Domain;

namespace LogicInterface
{
    public interface IUserLogic
    {
        User Login(string userName);
        bool PurchaseGame(User userLogged, int gameId);
    }
}