using CashBank.Domain.Entities;
using CashBank.Domain.Repositories.Customers;
using CashBank.Infrastructure.DataAcess;

namespace CashBank.Infrastructure.Repositories;

internal class CustomersRepository : ICustomersRepository
{
    public Task Add(Customer customer)
    {
        var dbContext = new CashBankContextDb();
        dbContext.Customers.Add(customer);
        return dbContext.SaveChangesAsync();
    }
}