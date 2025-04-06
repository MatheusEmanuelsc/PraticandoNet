

```md
# Repository Pattern com Unit of Work no ASP.NET Core 8

## Índice
1. [Visão Geral](#visão-geral)
2. [Repositório Específico (Normal)](#repositório-específico-normal)
3. [Repositório Genérico](#repositório-genérico)
4. [Repositório Parcialmente Genérico (Customizado)](#repositório-parcialmente-genérico-customizado)
5. [Unit of Work](#unit-of-work)
6. [Injeção e Uso no Controller](#injeção-e-uso-no-controller)
7. [Registro no Program.cs](#registro-no-programcs)
8. [Boas Práticas](#boas-práticas)

---

## Visão Geral

O padrão Repository centraliza a lógica de acesso a dados, enquanto o Unit of Work coordena a transação entre múltiplos repositórios. É uma forma limpa de isolar a camada de persistência da lógica de negócios.

---

## Repositório Específico (Normal)

**IUsuarioRepository.cs**
```csharp
public interface IUsuarioRepository
{
    Task<Usuario> GetByIdAsync(int id);
    Task AddAsync(Usuario usuario);
    // Outros métodos específicos
}
```

**UsuarioRepository.cs**
```csharp
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context) => _context = context;

    public async Task<Usuario> GetByIdAsync(int id) =>
        await _context.Usuarios.FindAsync(id);

    public async Task AddAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);
}
```

---

## Repositório Genérico

**IGenericRepository.cs**
```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

**GenericRepository.cs**
```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
}
```

---

## Repositório Parcialmente Genérico (Customizado)

**IUsuarioRepository.cs**
```csharp
public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetWithPedidosAsync(int id);
}
```

**UsuarioRepository.cs**
```csharp
public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario?> GetWithPedidosAsync(int id)
    {
        return await _dbSet.Include(u => u.Pedidos)
                           .FirstOrDefaultAsync(u => u.Id == id);
    }
}
```

---

## Unit of Work

**IUnitOfWork.cs**
```csharp
public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IGenericRepository<Pedido> Pedidos { get; }
    Task<int> CommitAsync();
}
```

**UnitOfWork.cs**
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IUsuarioRepository Usuarios { get; }
    public IGenericRepository<Pedido> Pedidos { get; }

    public UnitOfWork(AppDbContext context,
                      IUsuarioRepository usuarios,
                      IGenericRepository<Pedido> pedidos)
    {
        _context = context;
        Usuarios = usuarios;
        Pedidos = pedidos;
    }

    public async Task<int> CommitAsync() => await _context.SaveChangesAsync();
}
```

---

## Injeção e Uso no Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public UsuariosController(IUnitOfWork uow) => _uow = uow;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Usuario usuario)
    {
        await _uow.Usuarios.AddAsync(usuario);
        await _uow.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _uow.Usuarios.GetWithPedidosAsync(id);
        return usuario is null ? NotFound() : Ok(usuario);
    }
}
```

---

## Registro no Program.cs

```csharp
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

---

## Boas Práticas

- **Prefira o repositório específico quando tiver lógicas de domínio complexas.**
- **Use o repositório genérico para entidades simples.**
- **Crie repositórios parcialmente genéricos se precisar de ambos: base + métodos customizados.**
- **Unit of Work centraliza os commits e evita vazamento de responsabilidades.**
- **Não exponha `DbContext` fora da infraestrutura.**

