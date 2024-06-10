```markdown
# Padrão Repository e Unit of Work em .NET

## Índice
1. [Introdução](#introdução)
2. [Benefícios e Malefícios](#benefícios-e-malefícios)
3. [Implementando o Padrão Repository](#implementando-o-padrão-repository)
    1. [Interface Genérica](#interface-genérica)
    2. [Classe Genérica](#classe-genérica)
    3. [Classes Específicas](#classes-específicas)
    4. [Configuração dos Serviços](#configuração-dos-serviços)
4. [Implementando o Padrão Unit of Work](#implementando-o-padrão-unit-of-work)
    1. [Interface Unit of Work](#interface-unit-of-work)
    2. [Classe Unit of Work](#classe-unit-of-work)
    3. [Configuração dos Serviços](#configuração-dos-serviços-uow)
5. [Ajustando os Controladores](#ajustando-os-controladores)

## Introdução

Neste tutorial, vamos explorar os padrões Repository e Unit of Work no contexto de uma aplicação .NET. O objetivo é organizar o acesso aos dados e gerenciar transações de forma eficiente, promovendo um código mais limpo e de fácil manutenção.

## Benefícios e Malefícios

### Benefícios
- **Separação de Responsabilidades**: Organiza o código em camadas, separando a lógica de acesso aos dados da lógica de negócios.
- **Testabilidade**: Facilita a criação de testes unitários ao permitir o uso de mocks.
- **Manutenção**: Promove um código mais modular e fácil de manter.
- **Reutilização**: Permite reutilizar os mesmos métodos de acesso aos dados em diferentes partes da aplicação.

### Malefícios
- **Complexidade**: Adiciona uma camada extra de abstração, que pode aumentar a complexidade inicial do projeto.
- **Desempenho**: Pode introduzir overhead devido à abstração, impactando o desempenho em aplicações de alta demanda.

## Implementando o Padrão Repository

### Interface Genérica

Primeiro, criaremos uma interface genérica `IRepository<T>` que define os métodos comuns entre os repositórios.

```csharp
public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? Get(Expression<Func<T, bool>> predicate);
    T Create(T entity);
    T Update(T entity);
    T Delete(T entity);
}
```

### Classe Genérica

Em seguida, implementaremos a interface em uma classe genérica `Repository<T>`.

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<T> GetAll()
    {
        return _context.Set<T>().AsNoTracking().ToList();
    }

    public T? Get(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefault(predicate);
    }

    public T Create(T entity)
    {
        _context.Set<T>().Add(entity);
        return entity;
    }

    public T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        return entity;
    }
}
```

### Explicação do Código

- `Set<T>()`: Obtém um `DbSet<T>` para a entidade fornecida. É usado para realizar operações de CRUD no contexto do banco de dados.
- `AsNoTracking()`: Melhora o desempenho ao desabilitar o rastreamento de mudanças.
- `FirstOrDefault(predicate)`: Retorna o primeiro elemento que satisfaz o predicado ou `null`.

### Classes Específicas

Criamos repositórios específicos que herdam do genérico, adicionando métodos específicos conforme necessário.

```csharp
public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {        
    }
}

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context): base(context)
    {       
    }

    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }
}
```

### Configuração dos Serviços

Registramos os repositórios no contêiner de injeção de dependências.

```csharp
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

## Implementando o Padrão Unit of Work

### Interface Unit of Work

Definimos a interface `IUnitOfWork` que gerencia os repositórios e as transações.

```csharp
public interface IUnitOfWork : IDisposable
{
    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }
    void Commit();
}
```

### Classe Unit of Work

Implementamos a interface em uma classe `UnitOfWork`.

```csharp
public class UnitOfWork : IUnitOfWork
{
    private IProdutoRepository? _produtoRepo;
    private ICategoriaRepository? _categoriaRepo;
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IProdutoRepository ProdutoRepository
    {
        get
        {
            return _produtoRepo ??= new ProdutoRepository(_context);
        }
    }

    public ICategoriaRepository CategoriaRepository
    {
        get
        {
            return _categoriaRepo ??= new CategoriaRepository(_context);
        }
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Configuração dos Serviços UoW

Registramos a `UnitOfWork` no contêiner de injeção de dependências.

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Ajustando os Controladores

Finalmente, ajustamos os controladores para utilizar o `UnitOfWork`.

```csharp
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger)
    {
        _uof = uof;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias = _uof.CategoriaRepository.GetAll();
        return Ok(categorias);
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);
        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id= {id} não encontrada...");
            return NotFound($"Categoria com id= {id} não encontrada...");
        }
        return Ok(categoria);
    }

    [HttpPost]
    public ActionResult Post(Categoria categoria)
    {
        if (categoria is null)
        {
            _logger.LogWarning("Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
        _uof.Commit();

        return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            _logger.LogWarning("Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        _uof.CategoriaRepository.Update(categoria);
        _uof.Commit();

        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);
        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
        _uof.Commit();

        return Ok(categoriaExcluida);
    }
}

public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _uof;

    public ProdutosController(IUnitOfWork uof)
    {
        _uof = uof;
    }

    [HttpGet("produtos/{id}")]
    public ActionResult<IEnumerable<Produto>> GetProdutosCategoria(int id)
    {
        var produtos = _uof.ProdutoRepository.GetProdutosPorCategoria(id);  
        
        if (produtos is null)
            return NotFound();

        return Ok(produtos);    
    }

    [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
    {
        var produtos = _uof.ProdutoRepository.GetAll();
        if (produtos is null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }

    [HttpGet("{id}", Name = "ObterProduto")]
    public ActionResult<Produto> Get(int id)
    {
        var produto = _uof.ProdutoRepository.Get(c

 => c.ProdutoId == id);   
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }
        return Ok(produto);
    }

    [HttpPost]
    public ActionResult Post(Produto produto)
    {
        if (produto is null)
            return BadRequest();

        var novoProduto = _uof.ProdutoRepository.Create(produto);
        _uof.Commit();

        return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProduto);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Produto produto)
    {
        if (id != produto.ProdutoId)
        {
            return BadRequest();
        }

        var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
        _uof.Commit();

        return Ok(produtoAtualizado);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
        _uof.Commit();

        return Ok(produtoDeletado);
    }
}
```

Com isso, concluímos a implementação dos padrões Repository e Unit of Work. Agora, temos uma aplicação bem estruturada, com um acesso aos dados organizado e um gerenciamento de transações eficiente.
```