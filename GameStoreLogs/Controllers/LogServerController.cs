using System.Collections.Generic;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameStoreLogs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogServerController : ControllerBase
    {
        private readonly ILogger<LogServerController> _logger;
        private List<Log> 

        public LogServerController(ILogger<LogServerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Log> GetAll()
        {
            return null;
        }

        private void GetLogsFromRabbtMQ()
        {
            
        }
    }
}