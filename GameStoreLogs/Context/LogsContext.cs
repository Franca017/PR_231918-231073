using Domain;
using Microsoft.EntityFrameworkCore;

namespace GameStoreLogs.Context
{
    public class LogsContext : DbContext
    {
        public LogsContext(DbContextOptions<LogsContext> options)
            : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; } = null!;
    }
}