using System.Collections.Generic;
using System.Linq;
using Domain;
using RepositoryInterface;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users;
        private static readonly object Locker = new object();

        public UserRepository()
        {
            _users = new List<User>();
        }

        public User GetUser(string user)
        {
            lock (Locker)
            {
                return _users.Find(e => e.UserName.Equals(user));
            }
        }

        public User Add(User user)
        {
            lock (Locker)
            {
                var highestId = _users.Any() ? _users.Max(x => x.Id) : 0;
                user.Id = highestId + 1;
                _users.Add(user);
                return user;
            }
        }

        public List<Game> GetPurchasedGames(int userLoggedId)
        {
            lock (Locker)
            {
                return _users.Find(e => e.Id == userLoggedId).PurchasedGames;
            }
        }

        public User GetById(int requestId)
        {
            lock (Locker)
            {
                return _users.First(u => u.Id == requestId);
            }
        }

        public void Delete(int requestId)
        {
            lock (Locker)
            {
                var user = _users.First(u => u.Id == requestId);
                _users.Remove(user);
            }
        }
    }
}
