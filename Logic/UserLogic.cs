using System;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class UserLogic : IUserLogic
    {
        private IUserRepository userRepository;

        public UserLogic(IServiceProvider serviceProvider)
        {
            userRepository = serviceProvider.GetService<IUserRepository>();
            
        }

        public User Login(string userName)
        {
            var user = userRepository.GetUser(userName.ToLower());
            if (user == null)
            {
                user = new User(userName.ToLower(), DateTime.Now);
                userRepository.Add(user);
            }

            return user;
        }
    }
}