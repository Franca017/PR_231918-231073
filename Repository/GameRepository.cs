using System.Collections.Generic;
using System.Linq;
using Domain;
using RepositoryInterface;

namespace Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly List<Game> _games;
        private static readonly object Locker = new object();
        public GameRepository()
        {
            _games = new List<Game>();
        }

        public Game Add(Game game)
        {
            lock (Locker)
            {
                var highestId = _games.Any() ? _games.Max(x => x.Id) : 0;
                game.Id = highestId + 1;
                _games.Add(game);
                return game;
            }
        }

        public Game GetById(int gameId)
        {
            lock (Locker)
            {
                return _games.Exists(g => g.Id == gameId) ? _games.First(g => g.Id == gameId) : null;
            }
        }

        public void Delete(int gameId)
        {
            lock (Locker)
            {
                var game = _games.First(g => g.Id == gameId);
                _games.Remove(game);
            }
        }

        public List<Game> GetPublishedGames(User userLogged)
        {
            lock (Locker)
            {
                return _games.FindAll(e => e.Creator.Id.Equals(userLogged.Id));
            }
        }

        public List<Game> GetGamesOverRating(int minRating)
        {
            lock (Locker)
            {
                return _games.FindAll(e => e.Rating >= minRating);
            }
        }

        public List<Game> GetAll()
        {
            lock (Locker)
            {
                return _games;
            }
        }
    }
}
