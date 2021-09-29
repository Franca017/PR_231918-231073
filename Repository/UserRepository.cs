using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
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
    }
}
