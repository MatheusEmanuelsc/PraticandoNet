using Microsoft.EntityFrameworkCore;
using Master.Domain.Models;
using Master.Persistence.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Master.Persistence.Data;

public class MasterDbContext : IdentityDbContext<AppUser>
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Instrutor> Instrutores => Set<Instrutor>();
    public DbSet<Preco> Precos => Set<Preco>();
    public DbSet<Qualificacao> Qualificacoes => Set<Qualificacao>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<CursoInstrutor> CursoInstrutores => Set<CursoInstrutor>();
    public DbSet<CursoPreco> CursoPrecos => Set<CursoPreco>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CURSO
        modelBuilder.Entity<Curso>(entity =>
        {
            entity.ToTable("Cursos");

            entity.Property(c => c.Tituto)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(c => c.Descricao)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasMany(c => c.Fotos)
                .WithOne(f => f.Curso)
                .HasForeignKey(f => f.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Qualificacaos)
                .WithOne(q => q.Curso)
                .HasForeignKey(q => q.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Precos)
                .WithMany(p => p.Cursos)
                .UsingEntity<CursoPreco>(
                    j => j
                        .HasOne(cp => cp.Preco)
                        .WithMany(p => p.CursoPrecos)
                        .HasForeignKey(cp => cp.PrecoId),
                    j => j
                        .HasOne(cp => cp.Curso)
                        .WithMany(c => c.CursoPrecos)
                        .HasForeignKey(cp => cp.CursoId),
                    j =>
                    {
                        j.HasKey(t => new { t.CursoId, t.PrecoId });
                        j.ToTable("CursoPrecos");
                    }
                );

            entity.HasMany(c => c.Instrutores)
                .WithMany(i => i.Cursos)
                .UsingEntity<CursoInstrutor>(
                    j => j
                        .HasOne(ci => ci.Instrutor)
                        .WithMany()
                        .HasForeignKey(ci => ci.InstrutorId),
                    j => j
                        .HasOne(ci => ci.Curso)
                        .WithMany()
                        .HasForeignKey(ci => ci.CursoId),
                    j =>
                    {
                        j.HasKey(t => new { t.CursoId, t.InstrutorId });
                        j.ToTable("CursoInstrutores");
                    }
                );
        });

        // INSTRUTOR
        modelBuilder.Entity<Instrutor>(entity =>
        {
            entity.ToTable("Instrutores");

            entity.Property(i => i.Nome)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(i => i.Apelido)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(i => i.GrauAcademico)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        // PRECO
        modelBuilder.Entity<Preco>(entity =>
        {
            entity.ToTable("Precos");

            entity.Property(p => p.Nome)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(p => p.PrecoAtual)
                .HasPrecision(10, 2);

            entity.Property(p => p.PrecoPromocao)
                .HasPrecision(10, 2);
        });

        // QUALIFICACAO
        modelBuilder.Entity<Qualificacao>(entity =>
        {
            entity.ToTable("Qualificacoes");

            entity.Property(q => q.Aluno)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.Property(q => q.Comentario)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.Property(q => q.Pontuacao)
                .IsRequired();
        });

        // FOTO
        modelBuilder.Entity<Foto>(entity =>
        {
            entity.ToTable("Fotos");

            entity.Property(f => f.Url)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(f => f.Curso)
                .WithMany(c => c.Fotos)
                .HasForeignKey(f => f.CursoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CURSOINSTRUTOR
        modelBuilder.Entity<CursoInstrutor>(entity =>
        {
            entity.ToTable("CursoInstrutores");

            entity.HasKey(ci => new { ci.CursoId, ci.InstrutorId });
        });

        // CURSOPRECO
        modelBuilder.Entity<CursoPreco>(entity =>
        {
            entity.ToTable("CursoPrecos");

            entity.HasKey(cp => new { cp.CursoId, cp.PrecoId });
        });
    }
}
