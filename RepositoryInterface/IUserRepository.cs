using System.Collections.Generic;
using Domain;

namespace RepositoryInterface
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User GetUser(string user);
        User Add(User user);

        List<Game> GetPurchasedGames(int userLoggedId);
        User GetById(int requestId);
        void Delete(int requestId);
    }
}
