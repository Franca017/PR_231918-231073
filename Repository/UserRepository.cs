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

        public UserRepository(DbContext contexto)
        {
            this.users = new List<User>();
        }

        public void Add(User user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
