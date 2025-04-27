

# 📦 Estrutura Profissional com Múltiplos Projetos no ASP.NET Core

Este guia mostra como criar um projeto ASP.NET Core moderno com **múltiplos projetos organizados por camadas**, com **separação entre aplicação e testes** usando as pastas `src` e `tests`.

---

## 🗂️ Estrutura Final da Solução

```
MySolution/
│
├── src/
│   ├── MyApp.API/               # Projeto principal ASP.NET Core (camada de apresentação)
│   ├── MyApp.Application/       # Camada de aplicação (serviços, interfaces, casos de uso)
│   ├── MyApp.Domain/            # Entidades, regras de domínio, interfaces base
│   ├── MyApp.Infrastructure/    # Acesso a dados, serviços externos (implementações)
│
├── tests/
│   └── MyApp.Tests/             # Testes unitários e de integração
│
├── MySolution.sln               # Arquivo da solução
```

---

## ✅ Passo a Passo

### 1. Criar a Solução

```bash
dotnet new sln -n MySolution
cd MySolution
mkdir src tests
```

---

### 2. Criar os Projetos

```bash
cd src

dotnet new webapi      -n MyApp.API
dotnet new classlib    -n MyApp.Application
dotnet new classlib    -n MyApp.Domain
dotnet new classlib    -n MyApp.Infrastructure

cd ../tests
dotnet new xunit       -n MyApp.Tests
```

---

### 3. Adicionar os Projetos à Solução

```bash
cd ..
dotnet sln add src/MyApp.API/MyApp.API.csproj
dotnet sln add src/MyApp.Application/MyApp.Application.csproj
dotnet sln add src/MyApp.Domain/MyApp.Domain.csproj
dotnet sln add src/MyApp.Infrastructure/MyApp.Infrastructure.csproj
dotnet sln add tests/MyApp.Tests/MyApp.Tests.csproj
```

---

### 4. Adicionar Referências entre os Projetos

#### ✅ `MyApp.API.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\MyApp.Application\MyApp.Application.csproj" />
  <ProjectReference Include="..\MyApp.Infrastructure\MyApp.Infrastructure.csproj" />
</ItemGroup>
```

#### ✅ `MyApp.Application.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
</ItemGroup>
```

#### ✅ `MyApp.Infrastructure.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
</ItemGroup>
```

#### ✅ `MyApp.Tests.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\MyApp.Application\MyApp.Application.csproj" />
  <ProjectReference Include="..\..\src\MyApp.Domain\MyApp.Domain.csproj" />
</ItemGroup>
```

---

## 📌 O Que Vai em Cada Projeto

| Projeto                | Responsabilidades                                                                 |
|------------------------|------------------------------------------------------------------------------------|
| `MyApp.API`            | Controllers, configuração de serviços (DI), middlewares, endpoints                |
| `MyApp.Application`    | Serviços de aplicação, interfaces, regras de negócio, DTOs, validações            |
| `MyApp.Domain`         | Entidades, enums, interfaces base, lógica de domínio                              |
| `MyApp.Infrastructure` | Implementação de repositórios, banco de dados, serviços externos                  |
| `MyApp.Tests`          | Testes unitários (Application, Domain) e integração (API, banco de dados)         |

---

## 📦 Dependências Comuns

### 🔹 MyApp.API

```bash
dotnet add package Swashbuckle.AspNetCore
dotnet add package FluentValidation.AspNetCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### 🔹 MyApp.Application

```bash
dotnet add package MediatR
dotnet add package AutoMapper
dotnet add package FluentValidation
```

### 🔹 MyApp.Infrastructure

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### 🔹 MyApp.Tests

```bash
dotnet add package xunit
dotnet add package Moq
dotnet add package FluentAssertions
```

---

## 🔁 Comunicação entre Camadas (Responsabilidade por Fluxo)

1. `MyApp.API` chama um serviço da `Application`
2. `Application` usa interfaces da `Domain` para repositórios
3. `Infrastructure` implementa essas interfaces
4. `Domain` contém as entidades, regras e contratos

---

## 🔧 Exemplo Prático

### 🔸 MyApp.Domain

```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### 🔸 MyApp.Application

```csharp
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
}
```

### 🔸 MyApp.Infrastructure

```csharp
public class ProductService : IProductService
{
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return new List<Product> { new Product { Id = Guid.NewGuid(), Name = "Test" } };
    }
}
```

### 🔸 MyApp.API - Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _productService.GetAllAsync());
}
```

### 🔸 MyApp.API - Program.cs

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## ✅ Boas Práticas

- Use DTOs para comunicação entre camadas e mapeie com AutoMapper.
- Mantenha regras de negócio na Application, nunca nos Controllers.
- Teste logicamente o que está na `Domain` e `Application`.
- Use repositórios e Unit of Work na `Infrastructure`.

---

## 🚀 Rodar a API

```bash
dotnet run --project src/MyApp.API
```

---

## 📦 Dica Extra: Criar Template Personalizado

Você pode criar um template customizado com essa estrutura para usar sempre com:

```bash
dotnet new --install <nome-do-template>
```

Se quiser, posso montar isso para você. Deseja?

Ou quer agora que avancemos com um exemplo mais completo com EF Core, AutoMapper e FluentValidation integrados?