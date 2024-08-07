using Curso.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Curso.Api.Context
{
    public class AppDbContext : IdentityDbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Disciplina>? Disciplinas { get; set; }
        public DbSet<Aluno>? Alunos { get; set; }
    }
}
