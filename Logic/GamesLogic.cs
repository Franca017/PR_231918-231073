using System;
using System.Collections.Generic;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using RepositoryInterface;

namespace Logic
{
    public class GamesLogic : IGamesLogic
    {
        private readonly IGameRepository _gamesRepository;

        public GamesLogic(IServiceProvider serviceProvider)
        {
            _gamesRepository = serviceProvider.GetService<IGameRepository>();
            
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

        public void Delete(int gameId)
        {
            _gamesRepository.Delete(gameId);
        }

        public List<Game> GetPublishedGames(User userLogged)
        {
            return _gamesRepository.GetPublishedGames(userLogged);
        }

        public void Modify(string[] modifySplit)
        {
            var gameToModify = GetById(Convert.ToInt32(modifySplit[0]));
            if (!modifySplit[1].Equals("-")) gameToModify.Title = modifySplit[1];
            if (!modifySplit[2].Equals("-")) gameToModify.Genre = modifySplit[2];
            if (!modifySplit[3].Equals("-")) gameToModify.Sinopsis = modifySplit[3];
        }

        public void AdjustRating(int gameId, int newRating)
        {
            var game = GetById(gameId);
            game.Rating = newRating;
        }
    }
}