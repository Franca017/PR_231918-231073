using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RepositoryInterface
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        void Add(User user);

    }
}
