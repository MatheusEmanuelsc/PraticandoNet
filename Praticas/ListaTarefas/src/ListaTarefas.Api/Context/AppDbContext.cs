using ListaTarefas.Api.Entities;

using Microsoft.EntityFrameworkCore;

namespace ListaTarefas.Api.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options ):base(options)
    {
        
    }
    
   public  DbSet<Tarefa> Tarefas { get; set; }
   
   
  
}