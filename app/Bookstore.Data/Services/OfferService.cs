using Bookstore.Domain;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Repositories;

namespace Bookstore.Data.Offers
{
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
            return new DomainPaginatedList<Offer>(result.Items, result.PageIndex, result.TotalPages, result.TotalCount);
        }

        public async Task<IEnumerable<Offer>> GetOffersAsync(string sub)
        {
            return await offerRepository.ListAsync(sub);
        }

        private class DomainPaginatedList<T> : Bookstore.Domain.IPaginatedList<T>
        {
            public IReadOnlyList<T> Items { get; }
            public int PageIndex { get; }
            public int TotalPages { get; }
            public int TotalCount { get; }
            public bool HasPreviousPage => PageIndex > 1;
            public bool HasNextPage => PageIndex < TotalPages;

            public DomainPaginatedList(IReadOnlyList<T> items, int pageIndex, int totalPages, int totalCount)
            {
                Items = items;
                PageIndex = pageIndex;
                TotalPages = totalPages;
                TotalCount = totalCount;
            }
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