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
public class PaginatedList<T> : IPaginatedList<T>
{
    private readonly List<T> _items;
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = source.Count();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);

        _items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
    }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public Task PopulateAsync()
    {
        // Implement if needed
        return Task.CompletedTask;
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
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

    public Task<IPaginatedList<ReferenceDataItem>> ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
    {
        var query = dbContext.ReferenceData.AsQueryable();

        if (filters.ReferenceDataType.HasValue)
        {
            query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);
        }

        var result = new PaginatedList<ReferenceDataItem>(query, pageIndex, pageSize);

        return Task.FromResult<IPaginatedList<ReferenceDataItem>>(result);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
}