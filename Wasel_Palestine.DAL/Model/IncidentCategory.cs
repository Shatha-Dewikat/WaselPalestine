using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class IncidentCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }      
        public string NameAr { get; set; }     

        public DateTime? DeletedAt { get; set; }

        public string NameAr { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<Incident> Incidents { get; set; }
        public List<Report> Reports { get; set; }
        public List<AlertSubscription> AlertSubscriptions { get; set; }
        

    }
}
