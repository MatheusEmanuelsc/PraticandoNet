using Artigos.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Artigos.Context;

public class ArtigosDbContext  : IdentityDbContext<ApplicationUser>
{
    public ArtigosDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<Article> Artigos { get; set; } 
}