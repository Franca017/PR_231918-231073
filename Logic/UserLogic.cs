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
        private IGamesLogic _gamesLogic;

        public UserLogic(IServiceProvider serviceProvider)
        {
            userRepository = serviceProvider.GetService<IUserRepository>();
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
        }

        public User Login(string userName)
        {
            var user = userRepository.GetUser(userName.ToLower());
            if (user == null)
            {
                user = new User(userName.ToLower(), DateTime.Now);
                user = userRepository.Add(user);
            }

            return user;
        }

        public bool PurchaseGame(User userLogged, int gameId)
        {
            var game = _gamesLogic.GetById(gameId);
            if (!userLogged.PurchasedGames.Contains(game))
            {
                userLogged.PurchasedGames.Add(game);
                return true;
            }
            return false;
        }
    }
}