using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
        }

        public async Task<Incident?> GetByIdAsync(int id)
        {
            return await _context.Incidents
                .Include(i => i.Location)
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Incident>> GetAllAsync()
        {
            return await _context.Incidents
              .Include(i => i.Category)
              .Include(i => i.Severity)
              .Include(i => i.Status)
              .Include(i => i.Location)
              .ToListAsync();
        }

        public async Task DeleteAsync(Incident incident)
        {
            _context.Incidents.Remove(incident);
            if (incident != null)
            {
                _context.Incidents.Remove(incident);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Incident>> GetFilteredAsync(IncidentFilterRequest filter)
        {
            var query = _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .AsQueryable();

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId.Value);

            if (filter.SeverityId.HasValue)
                query = query.Where(i => i.SeverityId == filter.SeverityId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(i => i.StatusId == filter.StatusId.Value);


            if (filter.LocationName != null && !string.IsNullOrEmpty(filter.LocationName.AreaName))
                query = query.Where(i => i.Location.AreaName.Contains(filter.LocationName.AreaName));

            return await query.ToListAsync();
        }

        public async Task<List<Incident>> GetPagedAsync(PaginationRequest paginationRequest)
        {
            var page = paginationRequest.PageNumber < 1 ? 1 : paginationRequest.PageNumber;
            var pageSize = paginationRequest.PageSize < 1 ? 10 : paginationRequest.PageSize;

            return await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Incident>> GetFilteredPagedAsync(IncidentQueryRequest request)
        {
            var query = _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .AsQueryable();

            if (request.StatusId.HasValue)
                query = query.Where(i => i.StatusId == request.StatusId.Value);
            if (request.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == request.CategoryId.Value);
            if (request.SeverityId.HasValue)
                query = query.Where(i => i.SeverityId == request.SeverityId.Value);

           
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                if (request.SortDesc)
                    query = query.OrderByDescending(e => EF.Property<object>(e, request.SortBy));
                else
                    query = query.OrderBy(e => EF.Property<object>(e, request.SortBy));
            }
            else
            {
                query = query.OrderByDescending(i => i.CreatedAt); 
            }

            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                         .Take(request.PageSize);

            return await query.ToListAsync();
        }

    }
}
