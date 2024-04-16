using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using  _04_Database.Domain.Model;

namespace _04_Database.Domain
{
    public class AppDBContext : DbContext
    {
        //Tabela
       public DbSet<Atendimento> Atendimentos { get; set; } 
       public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
       {
        
       }
    }
}