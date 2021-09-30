using System;
using System.Collections.Generic;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IGamesLogic _gamesLogic;

        public UserLogic(IServiceProvider serviceProvider)
        {
            _userRepository = serviceProvider.GetService<IUserRepository>();
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
        }

        public User Login(string userName)
        {
            var user = _userRepository.GetUser(userName.ToLower());
            if (user == null)
            {
                user = new User(userName.ToLower(), DateTime.Now);
                user = _userRepository.Add(user);
            }

            return user;
        }

        public string PurchaseGame(User userLogged, int gameId)
        {
            var game = _gamesLogic.GetById(gameId);
            if (game == null)
            {
                return "The game has been deleted from the store.";
            }

            if (!userLogged.PurchasedGames.Contains(game))
            {
                userLogged.PurchasedGames.Add(game);
                return $"Game {gameId} was purchased by {userLogged.UserName}";
            }
            return $"The game {gameId} is already purchased by {userLogged.UserName}";
        }

        public List<Game> GetPurchasedGames(int userLoggedId)
        {
            List<Game> purchasedGames = _userRepository.GetPurchasedGames(userLoggedId);
            return purchasedGames;
        }
    }
}