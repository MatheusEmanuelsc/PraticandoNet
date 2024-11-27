## Configuração e Utilização do Pacote Sieve no .NET 8 com Entity Framework Core

### Índice
1. [Introdução ao Sieve](#1-introdução-ao-sieve)
2. [Configuração do Projeto](#2-configuração-do-projeto)
3. [Instalação do Pacote Sieve](#3-instalação-do-pacote-sieve)
4. [Configuração do Sieve](#4-configuração-do-sieve)
5. [Implementação do Filtro com Sieve](#5-implementação-do-filtro-com-sieve)
6. [Testando a Aplicação](#6-testando-a-aplicação)

### 1. Introdução ao Sieve
Sieve é um pacote útil para ASP.NET Core que facilita a implementação de filtros, ordenação e paginação em suas consultas de dados. Ele pode ser configurado para trabalhar com Entity Framework e outras fontes de dados.

### 2. Configuração do Projeto
Crie um novo projeto ASP.NET Core:

```bash
dotnet new webapi -n SieveExample
cd SieveExample
```

### 3. Instalação do Pacote Sieve
No diretório do projeto, instale o pacote Sieve:

```bash
dotnet add package Sieve
```

### 4. Configuração do Sieve

#### 4.1 Criação do Contexto do Banco de Dados
Crie uma pasta chamada `Data` e dentro dela, crie `ApplicationDbContext.cs`:

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

#### 4.2 Configuração no `Program.cs`
Configure o Entity Framework Core e o Sieve no `Program.cs`:

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

### 5. Implementação do Filtro com Sieve

#### 5.1 Configuração do Modelo
Crie a classe `Product`:

```csharp
using Sieve.Attributes;

public class Product
{
    public int Id { get; set; }
    
    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Price { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public DateTime CreatedDate { get; set; }
}
```

#### 5.2 Implementação do Repositório
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

#### 5.3 Configuração do Controlador
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

### 6. Testando a Aplicação
Execute a aplicação:

```bash
dotnet run
```

Faça uma requisição GET para a rota `/products` com os parâmetros de filtro desejados:

```http
GET /products?filters=Name@=Product1&sorts=Price
```

### Resumo Final
1. **Introdução ao Sieve**: Ferramenta para filtros, ordenação e paginação.
2. **Configuração do Projeto**: Criação do projeto ASP.NET Core.
3. **Instalação do Pacote Sieve**: Instalação do pacote via NuGet.
4. **Configuração do Sieve**: Criação do contexto do banco de dados e configuração do Sieve no `Program.cs`.
5. **Implementação do Filtro com Sieve**: Criação do modelo, repositório e controlador.
6. **Testando a Aplicação**: Execução e teste da aplicação.

Com estas etapas, você estará apto a configurar e utilizar o Sieve no seu projeto .NET 8 para filtrar, ordenar e paginar dados diretamente do banco de dados usando Entity Framework Core.