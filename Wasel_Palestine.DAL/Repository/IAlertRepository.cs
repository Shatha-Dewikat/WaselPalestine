using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IAlertRepository
    {
        public interface IAlertRepository
        {
            Task<Alert> CreateAsync(Alert alert);
            Task<Alert> GetByIdAsync(int id);
            Task<List<Alert>> GetAllAsync();
            Task<Alert> UpdateAsync(Alert alert);
            Task DeleteAsync(Alert alert);
        }
    }
}
