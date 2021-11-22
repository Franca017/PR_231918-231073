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

        public async Task<List<Log>> GetAll()
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
            var date = DateTime.MinValue;
            var dateFrom = DateTime.MinValue;
            var dateTo = DateTime.MinValue;
            try
            {
                date = DateTime.ParseExact(parameters.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    dateFrom = DateTime.ParseExact(parameters.DateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    dateTo = DateTime.ParseExact(parameters.DateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var logs = await GetAll();
            List<Log> filteredLogs = (List<Log>) logs;
            if (dateFrom != DateTime.MinValue && dateTo != DateTime.MinValue)
            {
                filteredLogs.RemoveAll(x => x.Date.Date > dateFrom.Date || x.Date.Date < dateTo.Date);
            }
            if(date != DateTime.MinValue)
            {
                filteredLogs.RemoveAll(x => !x.Date.Date.Equals(date));//Documentar que lo hacemos por dia
            }
            if(parameters.GameTitle != null)
            {
                var gameTitle = parameters.GameTitle.ToLower();
                filteredLogs.RemoveAll(x => !x.GameTitle.ToLower().Contains(gameTitle));
            }
            if(parameters.UserName != null)
            {
                var userName = parameters.UserName.ToLower();
                filteredLogs.RemoveAll(x => !x.User.ToLower().Contains(userName));
            }

            return filteredLogs;
        }
    }
}