using Amazon.Auth.AccessControlPolicy;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OfferRepository()
        {
            this.dbContext = ApplicationDbContext.GetDbContext();
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

            return await dbContext.Offer
                .GroupBy(x => 1)
                .Select(x => new OfferStatistics
                {
                    PendingOffers = x.Count(y => y.OfferStatus == OfferStatus.PendingApproval),
                    OffersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OffersTotal = x.Count()
                }).SingleOrDefaultAsync();
        }

        public async Task AddAsync(Offer offer)
        {
            await Task.Run(() => dbContext.Offer.Add(offer));
        }

        public Task<Offer> GetAsync(int id)
        {
            return dbContext.Offer.Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IPaginatedList<Offer>> ListAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Offer.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Author))
            {
                query = query.Where(x => x.Author.Contains(filters.Author));
            }

            if (!string.IsNullOrWhiteSpace(filters.BookName))
            {
                query = query.Where(x => x.BookName.Contains(filters.BookName));
            }

            if (filters.ConditionId.HasValue)
            {
                query = query.Where(x => x.ConditionId == filters.ConditionId);
            }

            if (filters.GenreId.HasValue)
            {
                query = query.Where(x => x.GenreId == filters.GenreId);
            }

            if (filters.OfferStatus.HasValue)
            {
                query = query.Where(x => x.OfferStatus == filters.OfferStatus);
            }

            query = query.Include(x => x.Customer)
                .Include(x => x.Condition)
                .Include(x => x.Genre);

            var result = await query.ToListAsync();
            var totalCount = await query.CountAsync();

            return new OfferPaginatedList(result, totalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Offer>> ListAsync(string sub)
        {
            return await dbContext.Offer
                .Include(x => x.BookType)
                .Include(x => x.Genre)
                .Include(x => x.Condition)
                .Include(x => x.Publisher)
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public class OfferPaginatedList : IPaginatedList<Offer>
    {
        private readonly List<Offer> _items;
        private readonly int _totalCount;
        private readonly int _pageIndex;
        private readonly int _pageSize;

        public OfferPaginatedList(List<Offer> items, int totalCount, int pageIndex, int pageSize)
        {
            _items = items;
            _totalCount = totalCount;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
        }

        public int PageIndex => _pageIndex;
        public int TotalPages => (int)Math.Ceiling(_totalCount / (double)_pageSize);
        public int TotalCount => _totalCount;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public int PageSize => _pageSize;
        public List<Offer> Items => _items;

        public IEnumerator<Offer> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}