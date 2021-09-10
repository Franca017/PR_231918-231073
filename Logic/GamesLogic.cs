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
        private static List<Game> Games { get; set; }
        private static IGameRepository gamesRepository;

        public GamesLogic(IServiceProvider serviceProvider)
        {
            gamesRepository = serviceProvider.GetService<IGameRepository>();
            
        }
        
        public List<Game> GetAll()
        {
            return (List<Game>) gamesRepository.GetAll();
        }
    }
}