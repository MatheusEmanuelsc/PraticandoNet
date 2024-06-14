using Microsoft.EntityFrameworkCore;
using SimpleBank.Models.Entitys;

namespace SimpleBank.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        DbSet<CurrentAccount>? CurrentAccounts { get; set; }
        DbSet<SaverAccount>? Savers { get; set; }
        DbSet<Transaction>? Transactions { get; set; }
    }
}
