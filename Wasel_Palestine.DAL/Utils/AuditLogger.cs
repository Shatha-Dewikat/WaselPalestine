using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Utils
{
    public class AuditLogger
    {
        private readonly ApplicationDbContext _db;
        public AuditLogger(ApplicationDbContext db) => _db = db;

        public async Task LogAsync(
            string userId,
            string action,
            string entityName,
            int entityId,
            string details,
            string ipAddress,
            string userAgent)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}