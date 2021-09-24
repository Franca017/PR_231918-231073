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

        public UserRepository()
        {
            this._users = new List<User>();
        }

        public User GetUser(string user)
        {
            return this._users.Find(e => e.UserName.Equals(user));
        }

        public User Add(User user)
        {
            var highestId = _users.Any() ? _users.Max(x => x.Id) : 0;
            user.Id = highestId + 1;
            _users.Add(user);
            return user;
        }

        public List<User> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
