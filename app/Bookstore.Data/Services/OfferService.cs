using Bookstore.Domain;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Repositories;
using System.Linq;

namespace Bookstore.Data.Offers
{
    public static class PaginatedListExtensions
    {
        public static Bookstore.Domain.IPaginatedList<T> ToDomainPaginatedList<T>(this Bookstore.Data.Repositories.IPaginatedList<T> list)
        {
            return new DomainPaginatedList<T>(list);
        }

        private class DomainPaginatedList<T> : Bookstore.Domain.IPaginatedList<T>
        {
            private readonly Bookstore.Data.Repositories.IPaginatedList<T> _innerList;

            public DomainPaginatedList(Bookstore.Data.Repositories.IPaginatedList<T> innerList)
            {
                _innerList = innerList;
            }

            public IReadOnlyList<T> Items => _innerList.Items;
            public int PageIndex => _innerList.PageIndex;
            public int TotalPages => _innerList.TotalPages;
            public int TotalCount => _innerList.TotalCount;
            public bool HasPreviousPage => _innerList.HasPreviousPage;
            public bool HasNextPage => _innerList.HasNextPage;

            public Task PopulateAsync() => Task.CompletedTask;
            public IList<int> GetPageList(int maxPages) => Enumerable.Range(1, Math.Min(maxPages, TotalPages)).ToList();
        }
    }

    public interface IOfferService
    {
        Task<Bookstore.Domain.IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<Offer>> GetOffersAsync(string sub);

        Task<Offer> GetOfferAsync(int offerId);

        Task CreateOfferAsync(CreateOfferDto createOfferDto);

        Task UpdateOfferStatusAsync(UpdateOfferStatusDto updateOfferStatusDto);

        Task<OfferStatistics> GetStatisticsAsync();
    }

    public class OfferService : IOfferService
    {
        private readonly Bookstore.Data.Repositories.IOfferRepository offerRepository;
        private readonly ICustomerRepository customerRepository;

        public OfferService()
        {
            this.offerRepository = InstanceCreator.GetOfferRepository();
            this.customerRepository = InstanceCreator.GetCustomerRepository();
        }

        public async Task<Bookstore.Domain.IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            var result = await offerRepository.ListAsync(filters, pageIndex, pageSize);
            return result.ToDomainPaginatedList();
        }

        public async Task<IEnumerable<Offer>> GetOffersAsync(string sub)
        {
            return await offerRepository.ListAsync(sub);
        }

        public async Task<Offer> GetOfferAsync(int id)
        {
            return await offerRepository.GetAsync(id);
        }

        public async Task CreateOfferAsync(CreateOfferDto dto)
        {
            var customer = await customerRepository.GetAsync(dto.CustomerSub);

            var offer = new Offer(
                customer.Id,
                dto.BookName,
                dto.Author,
                dto.ISBN,
                dto.BookTypeId,
                dto.ConditionId,
                dto.GenreId,
                dto.PublisherId,
                dto.BookPrice);

            await offerRepository.AddAsync(offer);

            await offerRepository.SaveChangesAsync();
        }

        public async Task UpdateOfferStatusAsync(UpdateOfferStatusDto dto)
        {
            var offer = await GetOfferAsync(dto.OfferId);

            offer.OfferStatus = dto.Status;

            offer.UpdatedOn = DateTime.UtcNow;

            await offerRepository.SaveChangesAsync();
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            return (await offerRepository.GetStatisticsAsync()) ?? new OfferStatistics();
        }
    }
}