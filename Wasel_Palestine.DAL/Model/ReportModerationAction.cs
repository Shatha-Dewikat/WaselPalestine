using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class ReportModerationAction
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; }

        public string ModeratorId { get; set; }
        public User Moderator { get; set; }

        public string Action { get; set; }
        public string Notes { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
