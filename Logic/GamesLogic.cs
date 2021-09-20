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
    }
}