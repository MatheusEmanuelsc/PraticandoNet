

# Tutorial de Criação de uma Web API em .NET 8 com MySQL, Entity Framework, AutoMapper, Repositories e Unit of Work

## Índice

- [Parte 0: Criação do Projeto e Adição de Pacotes](#parte-0-criação-do-projeto-e-adição-de-pacotes)
- [Parte 1: Conexão com o Banco de Dados](#parte-1-conexão-com-o-banco-de-dados)
- [Parte 2: Configuração do Repositório](#parte-2-configuração-do-repositório)
- [Parte 3: Unit of Work](#parte-3-unit-of-work)
- [Parte 4: AutoMapper](#parte-4-automapper)
- [Parte 5: Controllers](#parte-5-controllers)
- [Parte 6: Configuração Final](#parte-6-configuração-final)

## Parte 0: Criação do Projeto e Adição de Pacotes

### Criação do Projeto

```bash
dotnet new webapi -n Curso.Api
cd Curso.Api
```

### Adição de Pacotes

#### Pacotes do Banco de Dados (MySQL)

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

#### Pacote do ORM (EF)

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

#### Pacote para o Mapeamento

```bash
dotnet add package AutoMapper
```

## Parte 1: Conexão com o Banco de Dados

### Etapa 1: Configuração do Model e Relacionamentos

#### Model Disciplina

```csharp
namespace Curso.Api.Models
{
    public class Disciplina
    {
        [Key]
        public int DisciplinaId { get; set; }
        public int Aulas { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public ICollection<Aluno> Alunos { get; set; }
    }
}
```

#### Model Aluno

```csharp
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
```

### Etapa 2: Definição do Contexto e Tabelas

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Disciplina>? Disciplinas { get; set; }
    public DbSet<Aluno>? Alunos { get; set; }
}
```

### Etapa 3: Connection String e Ajuste no Program.cs

#### Configuração da Connection String

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DbCatalogo;Uid=myUsername;Pwd=SUASENHA;"
}
```

#### Ajuste no Program.cs

```csharp
var mysqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
});
```

### Etapa 4: Migração

#### Adicionar Migração

```bash
dotnet ef migrations add InitialMigration
```

#### Atualizar Banco de Dados

```bash
dotnet ef database update
```

## Parte 2: Configuração do Repositório

### Etapa 1: Interface do Repositório Genérico

```csharp
namespace Curso.Api.Repositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        T Create(T entity);
        T Update(T entity);
        T Delete(T entity);
    }
}
```

### Etapa 2: Implementação do Repositório Genérico

```csharp
namespace Curso.Api.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
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
```

### Etapa 3: Implementação de Repositórios Específicos (Opcional)

#### Interface Específica

```csharp
public interface IAlunoRepository : IRepository<Aluno> { }

namespace Curso.Api.Repositories
{
    public interface IDisciplinaRepository : IRepository<Disciplina> { }
}
```

#### Implementação da Interface Específica

```csharp
namespace Curso.Api.Repositories
{
    public class AlunoRepository : Repository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context) { }
    }

    public class DisciplinaRepository : Repository<Disciplina>, IDisciplinaRepository
    {
        public DisciplinaRepository(AppDbContext context) : base(context) { }
    }
}
```

#### Registro dos Repositórios no Program.cs

```csharp
builder.Services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

## Parte 3: Unit of Work

### Interface Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    IDisciplinaRepository DisciplinaRepository { get; }
    IAlunoRepository AlunoRepository { get; }
    Task CommitAsync();
}
```

### Implementação do Unit of Work

```csharp
public class UnitOfWork : IUnitOfWork
{
    private IDisciplinaRepository? _disciplinaRepo;
    private IAlunoRepository? _alunoRepository;
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IDisciplinaRepository DisciplinaRepository => _disciplinaRepo ??= new DisciplinaRepository(_context);
    public IAlunoRepository AlunoRepository => _alunoRepository ??= new AlunoRepository(_context);

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Registro do Unit of Work no Program.cs

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Parte 4: AutoMapper

### Etapa 1: DTOs

#### Aluno DTO

```csharp
public class AlunoDto
{
    [Key]
    public int AlunoId { get; set; }
    [Required]
    public string Nome { get; set; } = string.Empty;
    public double Nota { get; set; }
    public int? DisciplinaId { get; set; }
}
```

#### Disciplina DTO

```csharp
public class DisciplinaDto
{
    public int DisciplinaId { get; set; }
    public int Aulas { get; set; }
    [Required]
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
```

### Etapa 2: Profile do AutoMapper

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Aluno, AlunoDto>().ReverseMap();
        CreateMap<Disciplina, DisciplinaDto>().ReverseMap();
    }
}
```

### Etapa 3: Configuração do AutoMapper no Program.cs

```csharp
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

## Parte 5: Controllers

### Etapa 1: Criação do Controller

#### Controller de Disciplinas

```csharp
namespace Curso.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisciplinasController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DisciplinasController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
```

### Etapa 2: Definição dos Métodos

#### Método para Obter Lista de Disciplinas

```csharp
[HttpGet("disciplina/lista")]
public async Task<ActionResult<IEnumerable<DisciplinaDto>>> ListaDisciplinas() 
{
    var listaDisciplinas = await _unitOfWork.DisciplinaRepository.GetAllAsync();
    if (listaDisciplinas is null)
    {
        return NotFound("Não existem disciplinas...");
    }
   

 var disciplinasDto = _mapper.Map<IEnumerable<DisciplinaDto>>(listaDisciplinas);
    return Ok(disciplinasDto);
}
```

#### Método para Obter Disciplina por ID

```csharp
[HttpGet("disciplina/{id}", Name = "ObterDisciplina")]
public async Task<ActionResult<DisciplinaDto>> ObterDisciplina(int id)
{
    if (id <= 0)
    {
        return BadRequest("Valor inválido");
    }
    var disciplina = await _unitOfWork.DisciplinaRepository.GetAsync(d => d.DisciplinaId == id);
    if (disciplina is null)
    {
        return NotFound("Disciplina não encontrada...");
    }
    return Ok(_mapper.Map<DisciplinaDto>(disciplina));
}
```

#### Método para Criar Disciplina

```csharp
[HttpPost]
public async Task<ActionResult<DisciplinaDto>> CriarDisciplina(DisciplinaDto disciplinaDto) 
{
    if (disciplinaDto is null)
    {
        return BadRequest("Valor inválido");
    }
    var disciplina = _mapper.Map<Disciplina>(disciplinaDto);
    _unitOfWork.DisciplinaRepository.Create(disciplina);
    await _unitOfWork.CommitAsync();
    var novaDisciplinaDto = _mapper.Map<DisciplinaDto>(disciplina);
    return new CreatedAtRouteResult("ObterDisciplina",
        new { id = novaDisciplinaDto.DisciplinaId }, novaDisciplinaDto);
}
```

#### Método para Atualizar Disciplina

```csharp
[HttpPut("{id:int}")]
public async Task<ActionResult<DisciplinaDto>> AtualizarDisciplina(int id, DisciplinaDto disciplinaDto)
{
    if (id != disciplinaDto.DisciplinaId)
    {
        return BadRequest("Valor inválido");
    }
    var disciplina = _mapper.Map<Disciplina>(disciplinaDto);
    _unitOfWork.DisciplinaRepository.Update(disciplina);
    await _unitOfWork.CommitAsync();
    var disciplinaAtualizadaDto = _mapper.Map<DisciplinaDto>(disciplina);
    return Ok(disciplinaAtualizadaDto);
}
```

#### Método para Deletar Disciplina

```csharp
[HttpDelete("{id:int}")]
public async Task<ActionResult<DisciplinaDto>> DeletarDisciplina(int id)
{
    var disciplina = await _unitOfWork.DisciplinaRepository.GetAsync(c => c.DisciplinaId == id);
    if (disciplina is null)
    {
        return NotFound();
    }
    _unitOfWork.DisciplinaRepository.Delete(disciplina);
    await _unitOfWork.CommitAsync();
    var disciplinaDeletadaDto = _mapper.Map<DisciplinaDto>(disciplina);
    return Ok(disciplinaDeletadaDto);
}
```

## Parte 6: Configuração Final

### Configuração do JsonSerializer para Evitar Ciclo de Referência

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
```

Com isso, o tutorial de criação de uma Web API em .NET 8 está completo. Se precisar de mais detalhes ou exemplos adicionais, sinta-se à vontade para perguntar!