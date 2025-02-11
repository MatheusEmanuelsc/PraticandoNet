using CashBank.Domain.Entities;

namespace CashBank.Domain.Repositories.Customers;

public interface ICustomersRepository
{
    Task Add(Customer customer);
}