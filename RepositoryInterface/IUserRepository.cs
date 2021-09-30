using Domain;

namespace RepositoryInterface
{
    public interface IUserRepository
    {
        User GetUser(string user);
        User Add(User user);

    }
}
