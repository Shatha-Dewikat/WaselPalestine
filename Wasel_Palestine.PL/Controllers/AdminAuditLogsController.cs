using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;

namespace Wasel_Palestine.PL.Controllers
{
    [ApiController]
    [Route("api/admin/auditlogs")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminAuditLogsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/admin/auditlogs?userId=&action=&from=&to=&take=100
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? userId,
            [FromQuery] string? action,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int take = 100)
        {
            if (take <= 0) take = 100;
            if (take > 500) take = 500;

            var q = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                q = q.Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(action))
                q = q.Where(a => a.Action.Contains(action));

            if (from.HasValue)
                q = q.Where(a => a.Timestamp >= from.Value);

            if (to.HasValue)
                q = q.Where(a => a.Timestamp <= to.Value);

            var logs = await q
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.Action,
                    a.EntityName,
                    a.EntityId,
                    a.Timestamp,
                    a.IPAddress,
                    a.UserAgent,
                    a.Details
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}