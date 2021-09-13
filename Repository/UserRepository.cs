using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> users;

        public UserRepository()
        {
            this.users = new List<User>();
        }

        public User GetUser(string user)
        {
            return this.users.Find(e => e.UserName.Equals(user));
        }

        public void Add(User user)
        {
            this.users.Add(user);
        }

        public List<User> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
