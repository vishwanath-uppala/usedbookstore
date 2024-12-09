﻿using Bookstore.Domain.Addresses;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext dbContext;

        public AddressRepository()
        {
            this.dbContext = ServiceProvider.GetApplicationDbContext();
        }

        async Task IAddressRepository.DeleteAsync(string sub, int id)
        {
            var address = await dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id);

            if (address == null) return;

            address.IsActive = false;
        }

        async Task<Address> IAddressRepository.GetAsync(string sub, int id)
        {
            return await dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id && x.IsActive == true);
        }

        async Task<IEnumerable<Address>> IAddressRepository.ListAsync(string sub)
        {
            return await dbContext.Address.Where(x => x.Customer.Sub == sub && x.IsActive == true).ToListAsync();
        }

        async Task IAddressRepository.AddAsync(Address address)
        {
            await Task.Run(() => dbContext.Address.Add(address));
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}