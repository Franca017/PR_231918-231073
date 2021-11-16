using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        
        private readonly Greeter.GreeterClient _client;

        public UsersController()
        {
            _client = GrpcClient.Instance;
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
                new AddRequest() {Name = username});
            return reply.Message;
        }
    }
}