using System;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using LogicInterface;

namespace GameStoreGRPCServer.Services
{
    public class GameComsService : GameComs.GameComsBase
    {
        private readonly IGamesLogic _gamesLogic;
        public GameComsService(IGamesLogic gamesLogic)
        {
            _gamesLogic = gamesLogic;
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
            _gamesLogic.Modify(modifiedGame);
            return Task.FromResult(new GameReply()
            {
                Message = $"{modifiedGame[0]} was modified."
            });
        }
        
        public override Task<GameReply> DeleteGame(DeleteGameRequest request, ServerCallContext context)
        {
            var gameToDelete = request.Id;
            _gamesLogic.Delete(Int32.Parse(gameToDelete.ToString()));
            return Task.FromResult(new GameReply()
            {
                Message = $"Game with id {gameToDelete} was deleted from the store."
            });
        }
    }
}