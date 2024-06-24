using SimpleBank.Context;
using SimpleBank.Models.Entitys;

namespace SimpleBank.Repositories
{
    public class SaverAccountRepository:Repository<SaverAccount>,ISaverAccountRepository
    {
        public SaverAccountRepository(AppDbContext context):base(context) 
        {
            
        }
    }
}
