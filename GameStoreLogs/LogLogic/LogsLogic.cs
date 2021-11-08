using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using GameStoreLogs.Context;
using Microsoft.EntityFrameworkCore;

namespace GameStoreLogs.LogLogic
{
    public class LogsLogic : ILogsLogic
    {
        private readonly LogsContext _context;

        public LogsLogic(LogsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Log>> GetAll()
        {
            return await _context.Logs.ToListAsync();
        }

        public async Task<Log> GetLog(int id)
        {
            var log = await _context.Logs.FindAsync(id);

            return log;
        }

        public async void Add(Log log)
        {
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async void Delete(Log log)
        {
            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }
}