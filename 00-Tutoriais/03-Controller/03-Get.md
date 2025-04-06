

```md
# Actions GET com Unit of Work e Repository no ASP.NET Core

## Índice
1. [Introdução](#introdução)
2. [Cenários de Uso](#cenários-de-uso)
   - [Listar Todos](#listar-todos)
   - [Buscar por ID](#buscar-por-id)
   - [Incluir Relacionamentos](#incluir-relacionamentos)
   - [Com Filtros e Paginação](#com-filtros-e-paginação)
3. [Exemplo de Controller com UnitOfWork](#exemplo-de-controller-com-unitofwork)
4. [Program.cs e Injeção de Dependência](#programcs-e-injeção-de-dependência)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## Introdução

Actions GET no ASP.NET Core são usadas para recuperar dados de forma segura e idempotente. Quando integradas com os padrões **Repository** e **Unit of Work**, a aplicação ganha melhor separação de responsabilidades, testabilidade e coesão.

---

## Cenários de Uso

### Listar Todos
- Retorna todos os registros da entidade (ex: usuários).
- Idealmente, retorna um DTO com os campos relevantes, não o modelo completo.

### Buscar por ID
- Recupera um registro específico via rota (`[FromRoute]`).
- Deve retornar `404 NotFound` se não encontrado.

### Incluir Relacionamentos
- Pode retornar dados relacionados (ex: pedidos de um usuário).
- Recomendado deixar como opcional via query string.

### Com Filtros e Paginação
- Aceita parâmetros via `[FromQuery]` para filtro, ordenação e paginação.
- Permite buscas dinâmicas com performance e flexibilidade.

---

## Exemplo de Controller com UnitOfWork

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MeuProjeto.Dtos;
using MeuProjeto.Interfaces;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UsuariosController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // GET: api/usuarios
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var usuarios = await _uow.Usuarios.GetAllAsync();
        var usuariosDto = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
        return Ok(usuariosDto);
    }

    // GET: api/usuarios/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id);
        if (usuario is null) return NotFound();
        var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
        return Ok(usuarioDto);
    }

    // GET: api/usuarios/5/detalhes?includePedidos=true
    [HttpGet("{id}/detalhes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithDetailsAsync(int id, [FromQuery] bool includePedidos = false)
    {
        var usuario = await _uow.Usuarios.GetUsuarioComPedidosAsync(id, includePedidos);
        if (usuario is null) return NotFound();
        var dto = _mapper.Map<UsuarioDto>(usuario);
        return Ok(dto);
    }

    // GET: api/usuarios/filtrados?nome=Jo&page=1&pageSize=10
    [HttpGet("filtrados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilteredAsync(
        [FromQuery] string? nome,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var usuarios = await _uow.Usuarios.GetUsuariosFiltradosAsync(nome, page, pageSize);
        var dto = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
        return Ok(dto);
    }
}
```

---

## Program.cs e Injeção de Dependência

> Supondo que `IUnitOfWork`, `IUsuarioRepository`, `UsuarioRepository`, etc. já estejam implementados.

```csharp
var builder = WebApplication.CreateBuilder(args);

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Unit of Work e Repositórios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

---

## Boas Práticas

1. **Async/Await sempre**: Evite métodos síncronos em acesso a dados.
2. **DTOs na resposta**: Nunca exponha diretamente suas entidades.
3. **Repository isolado por agregado**: Um repositório por agregado raiz (ex: `Usuario`).
4. **Métodos especializados**: Use métodos como `GetUsuarioComPedidosAsync()` no repositório para carregar relacionamentos sob demanda.
5. **Query Params com `[FromQuery]`**: Use para filtros opcionais e paginação.
6. **Evite lógica no controller**: A lógica fica nos repositórios, serviços ou no AutoMapper, se aplicável.

---

## Conclusão

Combinando o ASP.NET Core com AutoMapper, DTOs, Repository e Unit of Work, suas Actions GET ficam mais limpas, testáveis e coesas. Este padrão melhora a manutenibilidade e segue as boas práticas de arquitetura em APIs RESTful modernas.
```
