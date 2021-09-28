using System.Collections.Generic;
using System.Linq;
using Domain;
using RepositoryInterface;

namespace Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly List<Game> _games;

        public GameRepository()
        {
            _games = new List<Game>();
            
        }

        public Game Add(Game game)
        {
            var highestId = _games.Any() ? _games.Max(x => x.Id) : 0;
            game.Id = highestId + 1;
            _games.Add(game);
            return game;
        }

        public Game GetById(int gameId)
        {
            return this._games.First(g => g.Id == gameId);
        }

        public void Delete(int gameId)
        {
            var game = GetById(gameId);
            _games.Remove(game);
        }

        public List<Game> GetPublishedGames(User userLogged)
        {
            return _games.FindAll(e => e.Creator.Id.Equals(userLogged.Id));
        }

        public List<Game> GetGamesOverRating(int minRating)
        {
            return _games.FindAll(e => e.Rating >= minRating);
        }

        public List<Game> GetAll()
        {
            return this._games;
        }
    }
}
