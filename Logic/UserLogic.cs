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
        private readonly ILogBuilderLogic _logBuilder;

        public UserLogic(IServiceProvider serviceProvider)
        {
            _userRepository = serviceProvider.GetService<IUserRepository>();
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _logBuilder = serviceProvider.GetService<ILogBuilderLogic>();
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
        
        public List<User> GetAll()
        {
            return _userRepository.GetAll();
        }
        
        public User GetById(int userId)
        {
            return _userRepository.GetById(userId);
        }

        public string PurchaseGame(User userLogged, int gameId, string userLoggedUserName)
        {
            var game = _gamesLogic.GetById(gameId);
            if (game == null)
            {
                return "The game has been deleted from the store.";
            }

            if (!userLogged.PurchasedGames.Contains(game))
            {
                userLogged.PurchasedGames.Add(game);
                _logBuilder.BuildLog(game, userLoggedUserName, "Purchase", $"The user {userLoggedUserName} purchased the game {game.Title}");
                return $"Game {gameId} was purchased by {userLogged.UserName}";
            }
            return $"The game {gameId} is already purchased by {userLogged.UserName}";
        }

        public List<Game> GetPurchasedGames(int userLoggedId)
        {
            List<Game> purchasedGames = _userRepository.GetPurchasedGames(userLoggedId);
            return purchasedGames;
        }

        public User Add(User newUser)
        {
            return _userRepository.Add(newUser);
        }
        public User Modify(int requestId, string requestName)
        {
            var userToModify = _userRepository.GetById(requestId);
            userToModify.UserName = requestName;
            return userToModify;
        }

        public string Delete(int requestId)
        {
            var user = GetById(requestId);
            if (user == null)
            {
                return "El usuario ingresado no existe en el sistema";
            }

            _userRepository.Delete(requestId);
            return $"User with id {requestId} was removed from the store.";
        }
        
        public string SellGame(User userLogged, int gameId)
        {
            var game = _gamesLogic.GetById(gameId);
            if (game == null)
            {
                return "The game has been deleted from the store.";
            }

            if (userLogged.PurchasedGames.Contains(game))
            {
                userLogged.PurchasedGames.Remove(game);
                return $"Game {gameId} was disassociated from {userLogged.UserName}";
            }
            return $"The game {gameId} is not a game purchased by {userLogged.UserName}";
        }
    }
}