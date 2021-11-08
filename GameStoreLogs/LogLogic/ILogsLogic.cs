using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace GameStoreLogs.LogLogic
{
    public interface ILogsLogic
    {
        Task<IEnumerable<Log>> GetAll();
        Task<Log> GetLog(int id);
        void Add(Log log);
        void Delete(Log log);
    }
}