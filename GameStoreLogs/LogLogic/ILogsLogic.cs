using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using GameStoreLogs.Model;

namespace GameStoreLogs.LogLogic
{
    public interface ILogsLogic
    {
        Task<List<Log>> GetAll();
        Task<Log> GetLog(int id);
        void Add(Log log);
        void Delete(Log log);
        Task<List<Log>> GetLogsFilteredAsync(ParametersModel parameters);
    }
}