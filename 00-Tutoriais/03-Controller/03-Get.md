

# Actions GET no ASP.NET Core

## Índice
1. [O que são Actions GET?](#o-que-são-actions-get)
2. [Cenários de Uso para GET](#cenários-de-uso-para-get)
   - [Listar Todos os Itens](#listar-todos-os-itens)
   - [Buscar por ID](#buscar-por-id)
   - [Incluir Relacionamentos](#incluir-relacionamentos)
   - [Outros Casos (Filtros, Paginação)](#outros-casos-filtros-paginação)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Controller com Actions GET](#passo-1-criar-o-controller-com-actions-get)
   - [Passo 2: Configurar no Program.cs](#passo-2-configurar-no-programcs)
   - [Passo 3: Testar os Endpoints](#passo-3-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Actions GET?

*Actions* GET são métodos em *Controllers* que lidam com requisições HTTP GET, usadas para recuperar dados sem alterar o estado do servidor. Elas geralmente retornam `IActionResult` ou `ActionResult<T>` e podem ser síncronas ou assíncronas.

---

## Cenários de Uso para GET

### Listar Todos os Itens
- **Descrição**: Retorna uma coleção de itens, como uma lista de usuários.
- **Recomendações**: Use `async` se houver acesso a banco ou I/O. `[FromQuery]` pode ser usado para filtros opcionais.

### Buscar por ID
- **Descrição**: Retorna um único item baseado em um identificador único.
- **Recomendações**: Use `[FromRoute]` para o ID e `async` para consultas ao banco. Retorne `404` se não encontrado.

### Incluir Relacionamentos
- **Descrição**: Retorna um item ou lista com dados relacionados (ex.: usuário e seus pedidos).
- **Recomendações**: Use `async` devido à complexidade das consultas. Considere parâmetros opcionais com `[FromQuery]` para incluir ou não relacionamentos.

### Outros Casos (Filtros, Paginação)
- **Descrição**: Permite busca filtrada ou paginada (ex.: por nome, página/tamanho).
- **Recomendações**: Use `[FromQuery]` para parâmetros de filtro/paginação e `async` para consultas dinâmicas.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Controller com Actions GET

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly List<UsuarioModel> _usuarios = new()
    {
        new() { Id = 1, Nome = "João", Pedidos = new() { new() { Id = 101, Produto = "Livro" } } },
        new() { Id = 2, Nome = "Maria", Pedidos = new() { new() { Id = 102, Produto = "Caneta" } } }
    };

    // GET: api/usuarios - Listar todos
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UsuarioModel>))]
    public async Task<IActionResult> GetAllAsync()
    {
        // Simula busca assíncrona
        await Task.Delay(50);
        return Ok(_usuarios);
    }

    // GET: api/usuarios/1 - Buscar por ID
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
        await Task.Delay(50); // Simula I/O
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // GET: api/usuarios/1/detalhes?includePedidos=true - Incluir relacionamentos
    [HttpGet("{id}/detalhes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithRelationshipsAsync([FromRoute] int id, [FromQuery] bool includePedidos = false)
    {
        await Task.Delay(50); // Simula I/O
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        if (!includePedidos)
            usuario.Pedidos = null; // Exclui pedidos se não solicitado

        return Ok(usuario);
    }

    // GET: api/usuarios/filtrados?nome=Jo&page=1&pageSize=10 - Filtros e paginação
    [HttpGet("filtrados")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UsuarioModel>))]
    public async Task<IActionResult> GetFilteredAsync(
        [FromQuery] string? nome,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        await Task.Delay(50); // Simula I/O
        var query = _usuarios.AsQueryable();

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(u => u.Nome.Contains(nome));

        var resultados = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(resultados);
    }
}

public class UsuarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public List<PedidoModel>? Pedidos { get; set; }
}

public class PedidoModel
{
    public int Id { get; set; }
    public string Produto { get; set; }
}
```

### Passo 2: Configurar no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

### Passo 3: Testar os Endpoints

- **GET /api/usuarios** (Listar todos):
  - Resposta: `200 OK` com `[{"id": 1, "nome": "João", "pedidos": [{"id": 101, "produto": "Livro"}]}, {"id": 2, "nome": "Maria", "pedidos": [{"id": 102, "produto": "Caneta"}]}]`
  
- **GET /api/usuarios/1** (Por ID):
  - Resposta: `200 OK` com `{"id": 1, "nome": "João", "pedidos": [{"id": 101, "produto": "Livro"}]}`
  - **GET /api/usuarios/999**: `404 NotFound`

- **GET /api/usuarios/1/detalhes** (Sem pedidos):
  - Resposta: `200 OK` com `{"id": 1, "nome": "João", "pedidos": null}`
- **GET /api/usuarios/1/detalhes?includePedidos=true** (Com pedidos):
  - Resposta: `200 OK` com `{"id": 1, "nome": "João", "pedidos": [{"id": 101, "produto": "Livro"}]}`

- **GET /api/usuarios/filtrados?nome=Jo&page=1&pageSize=1** (Filtrado e paginado):
  - Resposta: `200 OK` com `[{"id": 1, "nome": "João", "pedidos": [{"id": 101, "produto": "Livro"}]}]`

---

## Boas Práticas

1. **Use async para I/O**: Sempre aplique `async Task` em consultas a bancos ou APIs externas.
2. **Parâmetros explícitos**: Use `[FromRoute]` para IDs e `[FromQuery]` para filtros/paginação.
3. **Relacionamentos opcionais**: Inclua dados relacionados apenas se solicitado (ex.: via query string).
4. **Documente retornos**: Use `[ProducesResponseType]` para clareza em Swagger.
5. **Consistência REST**: Mantenha GET idempotente e sem efeitos colaterais.

---

## Conclusão

As *Actions* GET no ASP.NET Core cobrem desde listagens simples até cenários complexos com relacionamentos e filtros. Este tutorial abrange os casos principais — listar todos, buscar por ID, incluir relacionamentos e filtrar/paginar —, usando `async/await` para simular operações de I/O e atributos como `[FromQuery]` e `[ProducesResponseType]` para precisão. Todos os cenários são viáveis com ou sem tracking (ex.: Entity Framework), dependendo da necessidade de desempenho ou atualização.
