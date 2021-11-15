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

        private readonly ILogger<GamesController> _logger;

        public GamesController(ILogger<GamesController> logger)
        {
            _logger = logger;
        }

    }
}