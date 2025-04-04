

# Padrão Repository no ASP.NET Core

## Índice
1. [O que é o Padrão Repository?](#o-que-é-o-padrão-repository)
2. [Por que Usar?](#por-que-usar)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Definir o Modelo](#passo-1-definir-o-modelo)
   - [Passo 2: Criar a Interface do Repository](#passo-2-criar-a-interface-do-repository)
   - [Passo 3: Implementar o Repository](#passo-3-implementar-o-repository)
   - [Passo 4: Configurar Injeção de Dependência](#passo-4-configurar-injeção-de-dependência)
   - [Passo 5: Usar no Controller](#passo-5-usar-no-controller)
   - [Passo 6: Testar os Endpoints](#passo-6-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que é o Padrão Repository?

O padrão **Repository** é um padrão de design que abstrai a lógica de acesso a dados, atuando como uma camada intermediária entre a lógica de negócios e o armazenamento (ex.: banco de dados). Ele encapsula operações CRUD (Create, Read, Update, Delete) em uma interface reutilizável.

---

## Por que Usar?

- **Separação de preocupações**: Isola a lógica de dados da lógica de negócios.
- **Testabilidade**: Facilita a criação de mocks para testes unitários.
- **Manutenção**: Permite trocar o mecanismo de persistência (ex.: EF Core por outro ORM) sem alterar o código da aplicação.
- **Consistência**: Centraliza as operações de dados em um único ponto.

---

## Tutorial Passo a Passo

### Passo 1: Definir o Modelo

Crie um modelo simples para representar os dados.

```csharp
namespace MeuProjeto.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}
```

### Passo 2: Criar a Interface do Repository

Defina uma interface genérica para operações CRUD.

```csharp
using MeuProjeto.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuProjeto.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### Passo 3: Implementar o Repository

Crie uma implementação concreta usando uma lista em memória (poderia ser EF Core ou outro ORM).

```csharp
using MeuProjeto.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeuProjeto.Repositories;

public class UsuarioRepository : IRepository<Usuario>
{
    private readonly List<Usuario> _usuarios = new()
    {
        new() { Id = 1, Nome = "João", Email = "joao@email.com" },
        new() { Id = 2, Nome = "Maria", Email = "maria@email.com" }
    };

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        await Task.Delay(50); // Simula I/O
        return _usuarios;
    }

    public async Task<Usuario> GetByIdAsync(int id)
    {
        await Task.Delay(50); // Simula I/O
        return _usuarios.FirstOrDefault(u => u.Id == id);
    }

    public async Task AddAsync(Usuario usuario)
    {
        usuario.Id = _usuarios.Max(u => u.Id) + 1; // Gera ID fictício
        await Task.Delay(50); // Simula I/O
        _usuarios.Add(usuario);
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        var existente = await GetByIdAsync(usuario.Id);
        if (existente != null)
        {
            await Task.Delay(50); // Simula I/O
            existente.Nome = usuario.Nome;
            existente.Email = usuario.Email;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var usuario = await GetByIdAsync(id);
        if (usuario != null)
        {
            await Task.Delay(50); // Simula I/O
            _usuarios.Remove(usuario);
        }
    }
}
```

### Passo 4: Configurar Injeção de Dependência

Registre o *Repository* no contêiner de DI.

```csharp
using MeuProjeto.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Registra o Repository como scoped (padrão para EF Core também)
builder.Services.AddScoped<IRepository<Usuario>, UsuarioRepository>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

### Passo 5: Usar no Controller

Crie um *Controller* que utiliza o *Repository*.

```csharp
using MeuProjeto.Models;
using MeuProjeto.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IRepository<Usuario> _repository;

    public UsuariosController(IRepository<Usuario> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Usuario usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _repository.AddAsync(usuario);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] Usuario usuario)
    {
        if (id != usuario.Id || !ModelState.IsValid) return BadRequest();
        var existente = await _repository.GetByIdAsync(id);
        if (existente == null) return NotFound();
        await _repository.UpdateAsync(usuario);
        return Ok(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente == null) return NotFound();
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
```

### Passo 6: Testar os Endpoints

- **GET /api/usuarios**:
  - Resposta: `200 OK` com `[{"id": 1, "nome": "João", "email": "joao@email.com"}, {"id": 2, "nome": "Maria", "email": "maria@email.com"}]`
- **GET /api/usuarios/1**:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João", "email": "joao@email.com"}`
- **POST /api/usuarios** com `{ "nome": "Pedro", "email": "pedro@email.com" }`:
  - Resposta: `201 Created` com `{"id": 3, "nome": "Pedro", "email": "pedro@email.com"}`
- **PUT /api/usuarios/1** com `{ "id": 1, "nome": "João Atualizado", "email": "joao2@email.com" }`:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Atualizado", "email": "joao2@email.com"}`
- **DELETE /api/usuarios/1**:
  - Resposta: `204 No Content`

---

## Boas Práticas

1. **Interface genérica**: Use `IRepository<T>` para reutilização, mas crie interfaces específicas se houver operações únicas.
2. **Injeção de dependência**: Registre o *Repository* como `Scoped` para alinhar com o ciclo de vida do contexto de dados (ex.: EF Core).
3. **Assincronismo**: Sempre use `async/await` em operações de I/O para melhor escalabilidade.
4. **Validação**: Deixe a validação no *Controller* ou em camadas superiores, mantendo o *Repository* focado em dados.
5. **Evite excesso de abstração**: Não crie métodos desnecessários; ajuste o *Repository* às necessidades reais da aplicação.

---

## Conclusão

O padrão *Repository* no ASP.NET Core oferece uma abstração elegante para o acesso a dados, promovendo testabilidade e separação de preocupações. Este tutorial implementa um *Repository* genérico em memória, mas ele pode ser facilmente adaptado para EF Core ou outros ORMs. Integrado ao *Controller* via injeção de dependência, ele simplifica operações CRUD e mantém o código organizado.

