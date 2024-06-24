using SimpleBank.Repositories;

namespace SimpleBank.UnitOfWork
{
    public interface IUnitOfWork :IDisposable
    {
        ICurrentAccountRepository CurrentAccountRepository { get; }
        ISaverAccountRepository SaverAccountRepository { get; }
        ITransactionRepository TransactionRepository { get; }

        void Commit();
    }
}
