Vamos criar um padrão de repositório genérico em um projeto ASP.NET Core, incluindo a implementação de um endpoint em um controlador específico. Aqui está o passo a passo:

### 1. Criar um Novo Projeto ASP.NET Core

Primeiro, crie um novo projeto ASP.NET Core. No Visual Studio:

1. Abra o Visual Studio e clique em `Create a new project`.
2. Selecione `ASP.NET Core Web Application` e clique em `Next`.
3. Dê um nome ao seu projeto e clique em `Create`.
4. Selecione `API` e clique em `Create`.

### 2. Criar a Entidade

Crie uma classe de entidade chamada `Product`.

```csharp
namespace MyProject.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

### 3. Configurar o DbContext

Crie a classe de contexto do banco de dados que herda de `DbContext` no diretório `Data`.

```csharp
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
```

### 4. Criar a Interface Genérica do Repositório

Crie uma interface genérica de repositório no diretório `Repositories`.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
    }
}
```

### 5. Implementar a Classe Genérica do Repositório

Implemente a classe genérica do repositório no diretório `Repositories`.

```csharp
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            T entity = await GetById(id);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
```

### 6. Criar o Serviço

Crie um serviço para usar o repositório no diretório `Services`.

```csharp
using MyProject.Models;
using MyProject.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Services
{
    public class ProductService
    {
        private readonly IRepository<Product> _repository;

        public ProductService(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _repository.GetAll();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task AddProduct(Product product)
        {
            await _repository.Add(product);
        }

        public async Task UpdateProduct(Product product)
        {
            await _repository.Update(product);
        }

        public async Task DeleteProduct(int id)
        {
            await _repository.Delete(id);
        }
    }
}
```

### 7. Criar o Controlador

Crie um controlador específico para `Product` no diretório `Controllers`.

```csharp
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct(Product product)
        {
            await _productService.AddProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            await _productService.UpdateProduct(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}
```

### 8. Configurar a Injeção de Dependência

Configure a injeção de dependência no `Program.cs` (ou `Startup.cs` se estiver usando uma versão mais antiga do ASP.NET Core).

No arquivo `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ProductService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

No `appsettings.json`, configure sua string de conexão:

```json
"ConnectionStrings": {
    "DefaultConnection": "YourConnectionStringHere"
}
```

### Conclusão

Com esses passos, você terá um repositório genérico implementado e um endpoint para manipular entidades `Product` em um projeto ASP.NET Core. Este padrão ajuda a manter seu código organizado, reutilizável e fácil de testar.