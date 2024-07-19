using SimpleBank.Context;
using SimpleBank.Repositories;

namespace SimpleBank.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private ICurrentAccountRepository? _currentRepo;
        private ISaverAccountRepository? _saverRepo;
        private ITransactionRepository? _transactionRepo;
        private readonly AppDbContext _context;

        public ICurrentAccountRepository CurrentAccountRepository => _currentRepo ?? new CurrentAccountRepository(_context);

        public ISaverAccountRepository SaverAccountRepository => _saverRepo ?? new SaverAccountRepository(_context);

        public ITransactionRepository TransactionRepository => _transactionRepo ?? new TransactionRepository(_context);

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
