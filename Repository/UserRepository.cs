using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using RepositoryInterface;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
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
