using Amazon.Auth.AccessControlPolicy;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using System;
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

            var result = new PaginatedList<Offer>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return new PaginatedListWrapper<Offer>(result);
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
}