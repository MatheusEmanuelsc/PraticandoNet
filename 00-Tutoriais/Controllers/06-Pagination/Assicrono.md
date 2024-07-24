
### Passos para Implementar o Sieve com Operações Assíncronas

#### 1. Configuração do Contexto do Banco de Dados

Já configuramos o contexto do banco de dados anteriormente. Para garantir a continuidade, vamos usar a configuração que criamos:

```csharp
using Microsoft.EntityFrameworkCore;
using SieveExample.Models;

namespace SieveExample.Data
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

#### 2. Configuração do `Program.cs`

No arquivo `Program.cs`, configure o Entity Framework Core e o Sieve:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona o Sieve aos serviços
builder.Services.Configure<SieveOptions>(builder.Configuration.GetSection("Sieve"));
builder.Services.AddScoped<SieveProcessor>();

var app = builder.Build();
```

Adicione a string de conexão no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu_servidor;Database=sua_base_de_dados;User Id=seu_usuario;Password=sua_senha;"
  },
  "Sieve": {
    // Configurações do Sieve
  }
}
```

#### 3. Implementação do Repositório Assíncrono

Crie a pasta `Repositories` e dentro dela, crie `ProductRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using SieveExample.Data;
using SieveExample.Models;
using Sieve.Services;
using Sieve.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SieveExample.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public ProductRepository(ApplicationDbContext context, SieveProcessor sieveProcessor)
        {
            _context = context;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<List<Product>> GetProductsAsync(SieveModel sieveModel)
        {
            var query = _context.Products.AsQueryable();
            query = _sieveProcessor.Apply(sieveModel, query);
            return await query.ToListAsync();
        }
    }
}
```

#### 4. Configuração do Controlador

Crie a pasta `Controllers` e dentro dela, crie `ProductsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using SieveExample.Repositories;
using System.Threading.Tasks;

namespace SieveExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public ProductsController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] SieveModel sieveModel)
        {
            var products = await _productRepository.GetProductsAsync(sieveModel);
            return Ok(products);
        }
    }
}
```

### Testando a Aplicação

Execute a aplicação:

```bash
dotnet run
```

Faça uma requisição GET para a rota `/products` com os parâmetros de filtro desejados:

```http
GET /products?filters=Name@=Product1&sorts=Price
```

### Resumo

1. **Configuração do Contexto do Banco de Dados**: Criamos a classe `ApplicationDbContext` para acessar o banco de dados usando o Entity Framework Core.
2. **Configuração do `Program.cs`**: Configuramos o Entity Framework Core e o Sieve no `Program.cs`.
3. **Implementação do Repositório Assíncrono**: Implementamos a classe `ProductRepository` que utiliza o `ApplicationDbContext` e o `SieveProcessor` para aplicar filtros e ordenar os dados do banco de dados de forma assíncrona.
4. **Configuração do Controlador**: Criamos o controlador `ProductsController` para expor os dados filtrados e ordenados.
5. **Testando a Aplicação**: Executamos e testamos a aplicação para verificar se os filtros e a ordenação estão sendo aplicados corretamente de forma assíncrona.

Com estas etapas, você pode utilizar o Sieve para realizar operações assíncronas no seu projeto .NET 8, garantindo uma aplicação mais eficiente e responsiva. Se precisar de mais detalhes ou tiver alguma dúvida, estou à disposição!