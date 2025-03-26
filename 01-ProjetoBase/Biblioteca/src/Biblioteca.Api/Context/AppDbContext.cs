using Biblioteca.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Autor> Autores { get; set; }
}