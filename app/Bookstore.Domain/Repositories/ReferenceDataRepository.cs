using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReferenceDataRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(ReferenceDataItem item)
        {
            await Task.Run(() => dbContext.ReferenceData.Add(item));
        }

        public async Task<ReferenceDataItem> GetAsync(int id)
        {
            return await dbContext.ReferenceData.FindAsync(id);
        }

        public async Task<IEnumerable<ReferenceDataItem>> FullListAsync()
        {
            return await dbContext.ReferenceData.ToListAsync();
        }

        public async Task<IPaginatedList<ReferenceDataItem>> ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.ReferenceData.AsQueryable();

            if (filters.ReferenceDataType.HasValue)
            {
                query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<ReferenceDataItem>(items, totalCount, pageIndex, pageSize);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public class PaginatedList<T> : IPaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageIndex { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}