using Microsoft.AspNetCore.Mvc;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        // GET
        public IActionResult Index()
        {
            return null;
            
        }
        
    }
}