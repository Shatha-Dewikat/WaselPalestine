using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class ReportMedia
    {
       public int Id { get; set; }

    public int ReportId { get; set; }
    public Report Report { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string VoteType { get; set; }
    public DateTime CreatedAt { get; set; }
    }
}
