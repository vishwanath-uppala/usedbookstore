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
    public class PaginatedListWrapper<T> : IPaginatedList<T>
    {
        private readonly PaginatedList<T> _innerList;

        public PaginatedListWrapper(PaginatedList<T> innerList)
        {
            _innerList = innerList;
        }

        public int PageIndex => _innerList.PageIndex;
        public int TotalPages => _innerList.TotalPages;
        public int TotalCount => _innerList.TotalCount;
        public bool HasPreviousPage => _innerList.HasPreviousPage;
        public bool HasNextPage => _innerList.HasNextPage;
        public List<T> Items => _innerList.Items;
    }

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

            var result = new PaginatedList<ReferenceDataItem>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return new PaginatedListWrapper<ReferenceDataItem>(result);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}