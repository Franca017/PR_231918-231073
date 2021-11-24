using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain;
using GameStoreLogs.LogLogic;
using GameStoreLogs.Model;

namespace GameStoreLogs.Controllers
{
    [Route("api/logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ILogsLogic _logsLogic;

        public LogsController(ILogsLogic logsLogic)
        {
            _logsLogic = logsLogic;
        }

        [HttpGet]
        public async Task<ActionResult<List<Log>>> GetLogs(string game, string user, string dateFrom, 
            string dateTo, string date)
        {
            if (game == null && user == null && (dateFrom == null || dateTo == null) && date == null)
                return await _logsLogic.GetAll();
            
            var parameters = new ParametersModel(game,user,dateFrom,dateTo,date);
            var logs = await _logsLogic.GetLogsFilteredAsync(parameters);
            if (logs == null || !logs.Any())
            {
                return NoContent();
            }
            return logs;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> GetLog(int id)
        {
            var log = await _logsLogic.GetLog(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _logsLogic.GetLog(id);
            if (log == null)
            {
                return NotFound();
            }

            _logsLogic.Delete(log);

            return NoContent();
        }
    }
}
