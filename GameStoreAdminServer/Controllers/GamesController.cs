using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {

        private readonly Greeter.GreeterClient _client;

        public GamesController()
        {
            _client = GrpcClient.Instance;
        }

    }
}