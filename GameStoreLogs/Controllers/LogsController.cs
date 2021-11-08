using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain;
using GameStoreLogs.LogLogic;

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

        // GET: api/Logs
        [HttpGet]
        public async Task<IEnumerable<Log>> GetLogs()
        {
            return await _logsLogic.GetAll();
        }

        // GET: api/Logs/5
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

        // POST: api/Logs
        [HttpPost]
        public ActionResult<Log> PostLog(Log log)
        {
            _logsLogic.Add(log);

            return CreatedAtAction(nameof(GetLog), new { id = log.Id }, log);
        }

        // DELETE: api/Logs/5
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
