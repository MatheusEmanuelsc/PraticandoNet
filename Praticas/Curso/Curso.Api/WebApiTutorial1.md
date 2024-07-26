Nesse tutorial tenho como objetivo fazer um tutorial passo a passo de fazer uma  Web Api

parte  0

    Criação  de projeto e adc de pacotes

    adc pacotes 

    Pacotes do banco de dado(mysql)
        dotnet add package Microsoft.EntityFrameworkCore
        dotnet add package Microsoft.EntityFrameworkCore.Tools

    Pacote do orm(EF)
        dotnet add package Pomelo.EntityFrameworkCore.MySql

    Pacote para o mapeamento
        dotnet add package AutoMapper

Parte 1  conexao Banco de dados
    Etapa  1

     

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


configuração Repository 


    Etapa 1 faça a interface do repositorio generico

        namespace Curso.Api.Repositorys
        {
            public interface IRepository<T> 
            {
                Task<IEnumerable<T>>GetAllAsync();
                Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
                T Create(T entity);
                T Update(T entity);
                T Delete(T entity);

            }
        }

    Etapa 2 Faça implementação da interface

        
    namespace Curso.Api.Repositorys
    {
        public class Repository<T> : IRepository<T> where T : class
        {
            protected readonly AppDbContext _context;

            public Repository(AppDbContext context){
                _context = context;
            }

            public T Create(T entity)
            {
                _context.Set<T>().Add(entity);
            
                return entity;
            }

            public T Delete(T entity)
            {
                _context.Set<T>().Remove(entity);
                return entity;
            }

            public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
            {
               return await _context.Set<T>().FirstOrDefaultAsync(predicate);
            }

            public async Task<IEnumerable<T>> GetAllAsync()
            {
                return await _context.Set<T>().AsNoTracking().ToListAsync();
           
            }

            public T Update(T entity)
            {
                _context.Set<T>().Update(entity);
                return entity;
            }
        }
    }

    Etapa 3(Opcional) Implemente os repositorios especificos  
    aonde permite que vc adicione metodos especificos    para alguma cadegoria caso não seja necessario ter um emtodo diferente
    vc pode ingnorar essa etapa e pular para proxima

    Primeiro criei a interfaces

        nela vc pode criar os metodos extras que vc quer por especificamente nessa etapa


        public interface IAlunoRepository : IRepository<Aluno>
        {

        }

        namespace Curso.Api.Repositorys
    {
        public interface IDisciplinaRepository : IRepository<Disciplina>
        {

        }
    }

    Depois crie a implementação dos metodos e interface

    namespace Curso.Api.Repositorys
{
    public class AlunoRepository : Repository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context)
        {
        }

        
    }
}
    public class DisciplinaRepository : Repository<Disciplina>, IDisciplinaRepository
    {
        public DisciplinaRepository(AppDbContext context) : base(context)
        {
        }
    }

    E Por ultimo  adc no program para  

    builder.Services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
    builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

part 3 Unit of work

    Primeiro faça interface

        public interface IUnitOfWork : IDisposable
        {
            IDisciplinaRepository DisciplinaRepository { get; }
            IAlunoRepository AlunoRepository { get; }
            //Obs: a linha de codigo acima Opcional vc  deve utilizar ela se vc seguio o passo anterior do repository opcional tbm


            Task CommitAsync();
        }
    
    Depois implemente  a interface

    public class UnitOfWork : IUnitOfWork
    {
        public IDisciplinaRepository? _disciplinaRepo;

        public IAlunoRepository? _alunoRepository;
        // As duas linhas acimas seguem o mesmo esquema do opcional

        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        public IDisciplinaRepository DisciplinaRepository
        {
            get
            {
                return _disciplinaRepo ??= new DisciplinaRepository(_context);
            }
        }

        public IAlunoRepository AlunoRepository
        {
            get
            {
                return _alunoRepository ??= new AlunoRepository(_context);
            }
        }
        // mesmo coisa do opcional essa parte de cima

        public async Task CommitAsync()
        {
            await  _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
    Por ultimo adc no program
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
part 4
    Autor mapper

    etapa  0 Considerando que vc ja adc o pacote auto mapper no inicio

    etapa 1  Dto    

        Com base nos models crie os dto

                public class AlunoDto
                {
                    [Key]
                    public int AlunoId { get; set; }
                    [Required]
                    public string Nome { get; set; } = string.Empty;
                    public double Nota { get; set; }
                    public int? DisciplinaId { get; set; }
                }

    
            public class DisciplinaDto
            {
                
                public int DisciplinaId { get; set; }
                public int Aulas { get; set; }
                [Required]
                public string Nome { get; set; } = string.Empty;
                public string? Descricao { get; set; }
            }
    etapa 2  criei o profile

        public class MappingProfile:Profile
        {
            public MappingProfile()
            {
                CreateMap<Aluno, AlunoDto>().ReverseMap();
                CreateMap<Disciplina, DisciplinaDto>().ReverseMap();
            }
        }
    etapa 3 configure o autormapper no controller

        builder.Services.AddAutoMapper(typeof(MappingProfile));


passo 5 controllers

    Primeiro faça injeção de dependencia

    exemplo de injeção  autor mapper e uniwork
    namespace Curso.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisciplinasController : ControllerBase
    {

        private  readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DisciplinasController( IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

    }
}
 depois defina os metodos

passo 6 para evitar problemas ciclo

        builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);