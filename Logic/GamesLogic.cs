using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class GamesLogic : IGamesLogic
    {
        private readonly IGameRepository _gamesRepository;
        private readonly ILogBuilderLogic _logBuilder;

        public GamesLogic(IServiceProvider serviceProvider)
        {
            _gamesRepository = serviceProvider.GetService<IGameRepository>();
            _logBuilder = serviceProvider.GetService<ILogBuilderLogic>();
        }
        
        public List<Game> GetAll()
        {
            return _gamesRepository.GetAll();
        }

        public Game GetById(int gameId)
        {
            return _gamesRepository.GetById(gameId);
        }

        public Game Add(Game game)
        {
            if (game.Creator == null)
            {
                var adminUser = new User("ADMIN-USER", DateTime.Parse("22/11/2021"));
                game.Creator = adminUser;
            }
            _logBuilder.BuildLog(game, game.Creator.UserName, "Publish", $"The user {game.Creator.UserName} published the game {game.Title}");
            return _gamesRepository.Add(game);
        }

        public List<Game> GetSearchedGames(string keywords)
        {
            var allGames = GetAll();
            var ret = new List<Game>();
            var lowerKeywords = keywords.ToLower();
            var words = lowerKeywords.Split(" ");
            for (var i = 0; i < allGames.Count; i++)
            {
                for (var j = 0; j < words.Length; j++)
                {
                    if (allGames[i].Title.ToLower().Contains(words[j]) || allGames[i].Genre.ToLower().Contains(words[j]))
                    {
                        if (!ret.Contains(allGames[i]))
                        {
                            ret.Add(allGames[i]);
                        }
                    }
                }
            }

            return ret;
        }

        public string Delete(int gameId, string userLoggedUserName)
        {
            var game = GetById(gameId);
            if (game == null)
            {
                return "El juego ingresado no existe en el sistema";
            }
            
            _gamesRepository.Delete(gameId);
            _logBuilder.BuildLog(game, userLoggedUserName, "Delete", $"The user {userLoggedUserName} deleted the game {game.Title}");
            return $"Game with id {gameId} was deleted from the store.";
        }

        public List<Game> GetPublishedGames(User userLogged)
        {
            return _gamesRepository.GetPublishedGames(userLogged);
        }

        public void Modify(string[] modifySplit, string userLoggedUserName)
        {
            var gameToModify = GetById(Convert.ToInt32(modifySplit[0]));
            if (!modifySplit[1].Equals("-")) gameToModify.Title = modifySplit[1];
            if (!modifySplit[2].Equals("-")) gameToModify.Genre = modifySplit[2];
            if (!modifySplit[3].Equals("-")) gameToModify.Sinopsis = modifySplit[3];
            _logBuilder.BuildLog(gameToModify, userLoggedUserName, "Modify", $"The user {userLoggedUserName} modified the game {gameToModify.Title}");
        }

        public void AdjustRating(int gameId, int newRating, string userLoggedUserName)
        {
            var game = GetById(gameId);
            game.Rating = newRating;
            _logBuilder.BuildLog(game, userLoggedUserName, "Rate", $"The user {userLoggedUserName} rated the game {game.Title}");
        }

        public List<Game> GetGamesOverRating(int minRating)
        {
            return _gamesRepository.GetGamesOverRating(minRating);
        }

        public void ModifyImage(string[] modifySplit, string userLoggedUserName)
        {
            var gameToModify = GetById(Convert.ToInt32(modifySplit[0]));
            gameToModify.Image = modifySplit[1];
            _logBuilder.BuildLog(gameToModify, userLoggedUserName, "Modify image", $"The user {userLoggedUserName} modified the image of the game {gameToModify.Title}");
        }
    }
}