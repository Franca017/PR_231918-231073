using System.Collections.Generic;
using Domain;
using GameStoreLogs.LogRepository;

namespace GameStoreLogs.LogLogic
{
    public class LogsLogic : ILogsLogic
    {
        private readonly ILogsRepository _logsRepository;

        public LogsLogic(ILogsRepository logsRepository)
        {
            _logsRepository = logsRepository;
        }

        public IEnumerable<Log> GetAll()
        {
            return _logsRepository.GetAll();
        }

        public void Add(Log log)
        {
            _logsRepository.Add(log);
        }
    }
}