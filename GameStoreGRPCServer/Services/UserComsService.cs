using System;
using System.Threading.Tasks;
using Grpc.Core;
using LogicInterface;

namespace GameStoreGRPCServer.Services
{
    public class UserComsService : UserComs.UserComsBase
    {
        private readonly IUserLogic _userLogic;
        public UserComsService(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        public override Task<UserReply> AddUser(AddUserRequest request, ServerCallContext context)
        {
            var newUser = new Domain.User(request.Name, DateTime.Now);
            var userAdded = _userLogic.Add(newUser);
            return Task.FromResult(new UserReply()
            {
                Message = $"{userAdded.UserName} (created at {userAdded.DateCreated.Day}/{userAdded.DateCreated.Month}"
            });
        }
    }
}