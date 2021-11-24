using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Grpc.Core;
using LogicInterface;

namespace GameStoreGRPCServer.Services
{
    public class UserComsService : UserComs.UserComsBase
    {
        private readonly IUserLogic _userLogic;
        private const string AdminUserName = "ADMIN-USER";
        public UserComsService(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        
        public override Task<UsersReply> GetUsers(RequestUsers request, ServerCallContext context)
        {
            var users = _userLogic.GetAll();
            var usersOut = new List<UserOut>();
            foreach (var user in users)
            {
                var userAdd = new UserOut()
                {
                    DateCreated = user.DateCreated.ToString(CultureInfo.InvariantCulture),
                    Id = user.Id,
                    Name = user.UserName
                };
                usersOut.Add(userAdd);
            }

            return Task.FromResult(new UsersReply()
            {
                UsersList = {usersOut}
            });
        }
        
        public override Task<UserReply> AddUser(AddUserRequest request, ServerCallContext context)
        {
            var newUser = new Domain.User(request.Name, DateTime.Now);
            var userAdded = _userLogic.Add(newUser);
            return Task.FromResult(new UserReply()
            {
                Message = $"{userAdded.UserName} (created at {userAdded.DateCreated.Day}/{userAdded.DateCreated.Month})"
            });
        }
        
        public override Task<UserReply> ModifyUser(ModifyUserRequest request, ServerCallContext context)
        {
            var response = _userLogic.Modify(request.Id, request.Name);
            return Task.FromResult(new UserReply()
            {
                Message = response
            });
        }
        
        public override Task<UserReply> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var response = _userLogic.Delete(request.Id);
            return Task.FromResult(new UserReply()
            {
                Message = response
            });
        }
        
        public override async Task<UserReply> PurchaseGame(AssociateGameRequest request, ServerCallContext context)
        {
            var user = _userLogic.GetById(request.UserId);
            var purchase = _userLogic.PurchaseGame(user, request.GameId,AdminUserName);
            return await Task.FromResult(new UserReply()
            {
                Message = purchase
            });
        }
        
        public override async Task<UserReply> SellGame(AssociateGameRequest request, ServerCallContext context)
        {
            var user = _userLogic.GetById(request.UserId);
            var purchase = _userLogic.SellGame(user, request.GameId);
            return await Task.FromResult(new UserReply()
            {
                Message = purchase
            });
        }
    }
}