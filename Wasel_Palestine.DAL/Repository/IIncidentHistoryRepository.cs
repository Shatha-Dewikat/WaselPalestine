using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentHistoryRepository
    {
        Task AddAsync(IncidentHistory history);
    }

}
