using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class ReportStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Report> Reports { get; set; }
    }
}
