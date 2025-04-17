using ListaTarefas.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ListaTarefas.Api.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options ):base(options)
    {
        
    }
    
   public  DbSet<Tarefa> Tarefas { get; set; }
   public DbSet<RefreshToken> RefreshTokens { get; set; }
   
   protected override void OnModelCreating(ModelBuilder builder)
   {
       base.OnModelCreating(builder);

       // Configurações adicionais
       builder.Entity<RefreshToken>()
           .HasKey(rt => rt.Id);
       builder.Entity<RefreshToken>()
           .HasIndex(rt => rt.Token)
           .IsUnique();
   }
}