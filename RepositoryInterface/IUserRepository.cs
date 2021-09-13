using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User GetUser(string user);
        void Add(User user);

    }
}
