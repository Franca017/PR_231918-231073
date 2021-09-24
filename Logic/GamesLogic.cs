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
        private IGameRepository gamesRepository;

        public GamesLogic(IServiceProvider serviceProvider)
        {
            gamesRepository = serviceProvider.GetService<IGameRepository>();
            
        }
        
        public List<Game> GetAll()
        {
            return gamesRepository.GetAll();
        }

        public Game GetById(int gameId)
        {
            return gamesRepository.GetById(gameId);
        }

        public Game Add(Game game)
        {
            return gamesRepository.Add(game);
        }

        public List<Game> GetSearchedGames(string keywords)
        {
            var allGames = GetAll();
            var ret = new List<Game>();
            string lowerKeywords = keywords.ToLower();
            var words = lowerKeywords.Split(" ");
            for (int i = 0; i < allGames.Count; i++)
            {
                for (int j = 0; j < words.Length; j++)
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
            gamesRepository.Delete(gameId);
        }

        public List<Game> GetPublishedGames(User userLogged)
        {
            return gamesRepository.GetPublishedGames(userLogged);
        }

        public List<Review> GetGameReviews(int gameId)
        {
            return gamesRepository.GetGameReviews(gameId);
        }

        public void AddReviewToGame(Review newReview)
        {
            this.gamesRepository.AddReviewToGame(newReview);
        }
    }
}