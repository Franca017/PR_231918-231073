using System;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using LogicInterface;

namespace GameStoreGRPCServer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly IGamesLogic _gamesLogic;
        private readonly IUserLogic _userLogic;
        public GreeterService(IGamesLogic gamesLogic, IUserLogic userLogic)
        {
            _gamesLogic = gamesLogic;
            _userLogic = userLogic;
        }
        public override Task<AddReply> AddGame(AddRequest request, ServerCallContext context)
        {
            var newGame = new Game(request.Name, request.Genre, request.Sinopsis);
            //settearle un adminUser
            var gameAdded = _gamesLogic.Add(newGame);
            return Task.FromResult(new AddReply()
            {
                Message = $"{gameAdded.Title} was published to the store with id {gameAdded.Id}"
            });
        }
        public override Task<AddReply> AddUser(AddRequest request, ServerCallContext context)
        {
            var newUser = new User(request.Name, DateTime.Now);
            var userAdded = _userLogic.Add(newUser);
            return Task.FromResult(new AddReply()
            {
                Message = $"{userAdded.UserName} (created at {userAdded.DateCreated.Day}/{userAdded.DateCreated.Month}"
            });
        }

    }
}