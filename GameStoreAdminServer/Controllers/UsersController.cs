using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        
        private readonly UserComs.UserComsClient _client;

        public UsersController()
        {
            var channel = GrpcChannelAccess.Instance;
            _client = new UserComs.UserComsClient(channel);
        }

        [HttpGet]
        public async Task<string> GetUsers()
        {
            var reply = await _client.GetUsersAsync(
                new RequestUsers());
            return reply.UsersList.ToString();
        }
        
        [HttpPost]
        public async Task<string> AddUser([FromBody]string username)
        {
            var reply = await _client.AddUserAsync(
                new AddUserRequest() {Name = username});
            return reply.Message;
        }
        
        [HttpPut("{id}")]
        public async Task<string> ModifyUser([FromRoute]int id, [FromBody]string username)
        {
            var reply = await _client.ModifyUserAsync(
                new ModifyUserRequest() { 
                    Id = id,
                    Name = username});
            return reply.Message;
        }
        
        [HttpPut("{userid}/purchase")]
        public async Task<string> PurchaseGame([FromRoute]int userId, [FromBody]int gameId)
        {
            var reply = await _client.PurchaseGameAsync(
                new AssociateGameRequest() { 
                    UserId = userId,
                    GameId = gameId});
            return reply.Message;
        }
        
        [HttpPut("{userid}/sell")]
        public async Task<string> SellGame([FromRoute]int userId, [FromBody]int gameId)
        {
            var reply = await _client.SellGameAsync(
                new AssociateGameRequest() { 
                    UserId = userId,
                    GameId = gameId});
            return reply.Message;
        }
        
        [HttpDelete("{id}")]
        public async Task<string> DeleteUser([FromRoute]int id)
        {
            var reply = await _client.DeleteUserAsync(
                new DeleteUserRequest() { 
                    Id = id });
            return reply.Message;
        }
    }
}