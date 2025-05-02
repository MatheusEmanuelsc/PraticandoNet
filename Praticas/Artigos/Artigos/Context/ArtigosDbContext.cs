using Artigos.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Artigos.Context;

public class ArtigosDbContext :  DbContext
{
    public ArtigosDbContext(DbContextOptions<ArtigosDbContext> options) : base(options) { }

    
    public DbSet<Article> Artigos { get; set; } 
}