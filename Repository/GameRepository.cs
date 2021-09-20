using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;

namespace Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly List<Game> games;

        public GameRepository()
        {
            games = new List<Game>();
            Add(new Game("GTA 4", "Accion", "..."));
            Add(new Game("SnowRunner", "Conduccion", "..."));
        }

        public void Add(Game game)
        {
            var highestId = games.Any() ? games.Max(x => x.Id) : 0;
            game.Id = highestId + 1;
            games.Add(game);
        }

        public Game GetById(int gameId)
        {
            return this.games.First(g => g.Id == gameId);
        }

        public List<Game> GetAll()
        {
            return this.games;
        }
    }
}
