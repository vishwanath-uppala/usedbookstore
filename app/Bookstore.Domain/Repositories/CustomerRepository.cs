﻿using Bookstore.Domain.Customers;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CustomerRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task ICustomerRepository.AddAsync(Customer customer)
        {
            await Task.Run(() => dbContext.Customer.Add(customer));
        }

        async Task<Customer> ICustomerRepository.GetAsync(int id)
        {
            return await dbContext.Customer.FindAsync(id);
        }

        async Task<Customer> ICustomerRepository.GetAsync(string sub)
        {
            return await dbContext.Customer.SingleOrDefaultAsync(x => x.Sub == sub);
        }

        async Task ICustomerRepository.SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
