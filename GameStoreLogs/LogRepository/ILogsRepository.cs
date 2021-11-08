using System.Collections.Generic;
using Domain;

namespace GameStoreLogs.LogRepository
{
    public interface ILogsRepository
    {
        IEnumerable<Log> GetAll();
        void Add(Log log);
    }
}