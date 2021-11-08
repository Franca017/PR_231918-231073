using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Domain;
using GameStoreLogs.Context;
using GameStoreLogs.Model;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<List<Log>> GetLogsFilteredAsync(ParametersModel parameters)
        {
            var gameTitle = parameters.GameTitle.ToLower();
            var userName = parameters.UserName.ToLower();
            var dateTime = new DateTime();
            try
            {
                dateTime = DateTime.ParseExact(parameters.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                dateTime = DateTime.MinValue;
            }
            
            var logs = await GetAll();
            List<Log> filteredLogs = (List<Log>) logs;
            if(dateTime != DateTime.MinValue)
            {
                filteredLogs.RemoveAll(x => !x.Date.Date.Equals(dateTime));//Documentar que lo hacemos por dia
            }
            if(gameTitle != "")
            {
                filteredLogs.RemoveAll(x => !x.GameTitle.ToLower().Contains(gameTitle));
            }
            if(userName != "")
            {
                filteredLogs.RemoveAll(x => !x.User.ToLower().Contains(userName));
            }

            return filteredLogs;
        }
    }
}