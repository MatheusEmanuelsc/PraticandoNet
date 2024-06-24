using SimpleBank.Context;
using SimpleBank.Models.Entitys;

namespace SimpleBank.Repositories
{
    public class CurrentAccountRepository: Repository<CurrentAccount>,ICurrentAccountRepository
    {
        public CurrentAccountRepository(AppDbContext context):base(context)
        {
            
        }
    }
}
