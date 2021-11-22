using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using LogicInterface;

namespace GameStoreGRPCServer.Services
{
    public class GameComsService : GameComs.GameComsBase
    {
        private readonly IGamesLogic _gamesLogic;
        private const string AdminUserName = "ADMIN-USER";

        public GameComsService(IGamesLogic gamesLogic)
        {
            _gamesLogic = gamesLogic;
        }
        
        
        public override Task<GamesReply> GetGames(RequestGames request, ServerCallContext context)
        {
            var games = _gamesLogic.GetAll();
            var gamesOut = new List<GameOut>();
            foreach (var game in games)
            {
                GameOut gameAdd = new GameOut()
                {
                    Id = game.Id,
                    Name = game.Title,
                    Genre = game.Genre,
                    Rating = game.Rating,
                    Sinopsis = game.Sinopsis
                };
                gamesOut.Add(gameAdd);
            }

            return Task.FromResult(new GamesReply()
            {
                GamesList = {gamesOut}
            });
        }
        
        public override Task<GameReply> AddGame(AddGameRequest request, ServerCallContext context)
        {
            var newGame = new Game(request.Name, request.Genre, request.Sinopsis);
            var gameAdded = _gamesLogic.Add(newGame);
            return Task.FromResult(new GameReply()
            {
                Message = $"{gameAdded.Title} was published to the store with id {gameAdded.Id}"
            });
        }
        
        public override Task<GameReply> ModifyGame(ModifyGameRequest request, ServerCallContext context)
        {
            string[] modifiedGame = {request.Id.ToString(), request.Name, request.Genre, request.Sinopsis};
            _gamesLogic.Modify(modifiedGame,AdminUserName);
            return Task.FromResult(new GameReply()
            {
                Message = $"{modifiedGame[0]} was modified."
            });
        }
        
        public override Task<GameReply> DeleteGame(DeleteGameRequest request, ServerCallContext context)
        {
            var gameToDelete = request.Id;
            var response = _gamesLogic.Delete(Int32.Parse(gameToDelete.ToString()),AdminUserName);
            return Task.FromResult(new GameReply()
            {
                Message = response
            });
        }
    }
}