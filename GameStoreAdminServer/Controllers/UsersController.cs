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
        
        // GET
        public IActionResult Index()
        {
            return null;
            
        }
        
        [HttpPost]
        public async Task<string> AddUser([FromBody]string username)
        {
            var reply = await _client.AddUserAsync(
                new AddUserRequest() {Name = username});
            return reply.Message;
        }
    }
}