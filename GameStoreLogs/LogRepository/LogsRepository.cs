
using System.Collections.Generic;
using Domain;

namespace GameStoreLogs.LogRepository
{
    public class LogsRepository : ILogsRepository
    {
        private List<Log> _logs;
        
        public LogsRepository()
        {
            _logs = new List<Log>();
        }

        public IEnumerable<Log> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public void Add(Log log)
        {
            throw new System.NotImplementedException();
        }
    }
}