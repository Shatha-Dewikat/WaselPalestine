using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Utils
{
    public class ReportStatusSeedData : ISeedData
    {
        private readonly ApplicationDbContext _context;

        public ReportStatusSeedData(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DataSeed()
        {
            if (!await _context.ReportStatuses.AnyAsync())
            {
                var reportStatuses = new List<ReportStatus>
                {
                    new ReportStatus { Name = "Pending" },
                    new ReportStatus { Name = "Approved" },
                    new ReportStatus { Name = "Rejected" },
                    new ReportStatus { Name = "UnderReview" }
                };

                await _context.ReportStatuses.AddRangeAsync(reportStatuses);
                await _context.SaveChangesAsync();
            }
        }
    }
}
