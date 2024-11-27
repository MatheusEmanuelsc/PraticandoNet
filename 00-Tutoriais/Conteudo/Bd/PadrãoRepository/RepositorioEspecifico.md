# Tutorial sobre o Padrão Repository

## Introdução
O padrão Repository é um padrão de design utilizado para abstrair a camada de acesso a dados, permitindo que a lógica de negócios interaja com os dados de forma desacoplada do mecanismo de armazenamento. Neste tutorial, vamos implementar o padrão Repository em um projeto ASP.NET Core utilizando Entity Framework Core.

## Passo a Passo

### 1. Criar a Interface do Repositório

Primeiramente, criamos uma interface para o repositório. As interfaces no C# são geralmente prefixadas com "I" e, no contexto de repositórios, são sufixadas com "Repository". Esta interface define os métodos que o repositório deve implementar.

```csharp
public interface ICategoriaRepository
{
    IEnumerable<Categoria> GetCategorias();
    Categoria GetCategoria(int id);
    Categoria Create(Categoria categoria); 
    Categoria Update(Categoria categoria);
    Categoria Delete(int id);
}
```

### 2. Implementar a Interface do Repositório

Em seguida, implementamos a interface. A implementação do repositório contém a lógica para interagir com o banco de dados utilizando o Entity Framework Core.

```csharp
public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Categoria> GetCategorias()
    {
        return _context.Categorias.ToList();
    }

    public Categoria GetCategoria(int id)
    {
        return _context.Categorias.FirstOrDefault(x => x.CategoriaId == id);
    }

    public Categoria Create(Categoria categoria)
    {
        if (categoria is null)
        {
            throw new ArgumentNullException(nameof(categoria));
        }
        _context.Categorias.Add(categoria);
        _context.SaveChanges();
        return categoria;
    }

    public Categoria Update(Categoria categoria)
    {
        if (categoria is null)
        {
            throw new ArgumentNullException(nameof(categoria));
        }
        _context.Entry(categoria).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        _context.SaveChanges();
        return categoria;
    }

    public Categoria Delete(int id)
    {
        var categoria = _context.Categorias.Find(id);
        if (categoria is null)
        {
            throw new ArgumentNullException(nameof(categoria));
        }
        _context.Remove(categoria);
        _context.SaveChanges();
        return categoria;
    }
}
```

### 3. Utilizar o Repositório na Controller

Ao invés de interagir diretamente com o contexto do banco de dados, a controller deve chamar os métodos do repositório, que encapsulam a lógica de acesso a dados.

```csharp
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaRepository repository, ILogger<CategoriasController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias = _repository.GetCategorias();
        return Ok(categorias);
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        var categoria = _repository.GetCategoria(id);

        if (categoria == null)
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
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        _repository.Create(categoria);

        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        _repository.Update(categoria);
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _repository.GetCategoria(id);

        if (categoria == null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaExcluida = _repository.Delete(id);
        return Ok(categoriaExcluida);
    }
}
```

### 4. Configurar a Injeção de Dependência

Por fim, registramos a implementação do repositório no container de injeção de dependência no arquivo `Program.cs`.

```csharp
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
```

## Conclusão

O padrão Repository ajuda a manter a lógica de acesso a dados separada da lógica de negócios, promovendo um código mais limpo e de fácil manutenção. Este tutorial apresentou uma implementação básica desse padrão em um projeto ASP.NET Core com Entity Framework Core, mas o padrão pode ser expandido e adaptado conforme as necessidades específicas do seu projeto.