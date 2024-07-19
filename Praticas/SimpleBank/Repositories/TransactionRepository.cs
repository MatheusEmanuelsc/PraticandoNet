using SimpleBank.Context;
using SimpleBank.Models.Entitys;

namespace SimpleBank.Repositories
{
    public class TransactionRepository:Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context):base(context)
        {
            
        }
    }
}
