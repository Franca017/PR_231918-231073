using System.Collections.Generic;
using Domain;

namespace GameStoreLogs.LogLogic
{
    public interface ILogsLogic
    {
        IEnumerable<Log> GetAll();
        void Add(Log log);
    }
}