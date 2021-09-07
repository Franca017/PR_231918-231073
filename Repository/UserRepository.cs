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
        private readonly DbContext contexto;
        private readonly DbSet<User> users;

        public UserRepository(DbContext contexto)
        {
            this.contexto = contexto;
            this.users = contexto.Set<User>();
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
