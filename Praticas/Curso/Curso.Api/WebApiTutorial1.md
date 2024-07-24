Nesse tutorial tenho como objetivo fazer um tutorial passo a passo de fazer uma  Web Api


Parte 1  conexao Banco de dados
    Etapa  1

    adc pacotes 

    Pacotes do banco de dado(mysql)
        dotnet add package Microsoft.EntityFrameworkCore
        dotnet add package Microsoft.EntityFrameworkCore.Tools

    Pacote do orm(EF)
        dotnet add package Pomelo.EntityFrameworkCore.MySql

    Pacote para o mapeamento
        dotnet add package AutoMapper

    Etapa 2

        Faça o Model e defina os relacionamentos  

        Exemplos de models e relacionamentos

        namespace Curso.Api.Models
    {
        public class Disciplina
        {
            [Key]
            public int DisciplinaId { get; set; }
            public int Aulas { get; set; }
            public string Nome { get; set; }=string.Empty;
            public string? Descricao { get; set; }

            public ICollection<Aluno> Alunos { get; set; }
        }
    }



        
    namespace Curso.Api.Models
    {
        public class Aluno
        {
            [Key]
            public int AlunoId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public decimal Nota { get; set; }

            public int DisciplinaId { get; set; }

            
            public Disciplina? Disciplina { get; set; }

        }
    }


    Etapa 3

        Defina o context  e tabelas

        public class AppDbContext : DbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Disciplina>? Disciplinas { get; set; }
        public DbSet<Aluno>? Alunos { get; set; }
    }


    Etapa 4

        Faça connectionString  e ajuste  o program

        "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=DbCatalogo;Uid=myUsername;Pwd=SUASENHA;"
    }


    Etapa 5  ajuste o program

        var mysqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
        });

    Etapa 6 

        Faça a migration

        dotnet ef migrations add <NomeDaMigracao>

        depois criei o banco de dados

        dotnet ef database update

parte 2


configuração bd e uniofworks

part 3


para evitar problemas ciclo

builder.Services.AddControllers()
  .AddJsonOptions(options =>
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);