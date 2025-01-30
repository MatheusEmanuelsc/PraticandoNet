namespace CashBank.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
}