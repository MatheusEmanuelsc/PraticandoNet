

# Controllers no ASP.NET Core

## Índice
1. [O que são Controllers?](#o-que-são-controllers)
2. [Controllers no ASP.NET Core](#controllers-no-aspnet-core)
   - [Funcionalidades Principais](#funcionalidades-principais)
   - [Tipos de Controllers](#tipos-de-controllers)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar um Controller Simples](#passo-1-criar-um-controller-simples)
   - [Passo 2: Configurar no Program.cs](#passo-2-configurar-no-programcs)
   - [Passo 3: Testar os Endpoints](#passo-3-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Controllers?

*Controllers* são classes responsáveis por processar requisições HTTP, coordenar a lógica de negócios e retornar respostas ao cliente. Eles são o ponto central do padrão MVC (*Model-View-Controller*) ou da construção de APIs no ASP.NET Core.

---

## Controllers no ASP.NET Core

### Funcionalidades Principais
- **Roteamento**: Mapeiam URLs para ações específicas.
- **Model Binding**: Convertem dados da requisição em parâmetros ou modelos.
- **Validação**: Integram-se ao `ModelState` para verificar dados de entrada.
- **Respostas**: Retornam resultados como JSON, HTML ou códigos de status HTTP via `IActionResult`.

### Tipos de Controllers
- **API Controllers**: Usados em APIs RESTful, herdando de `ControllerBase` e decorados com `[ApiController]`.
- **MVC Controllers**: Usados em aplicações web com views, herdando de `Controller` para suporte a renderização de páginas.

---

## Tutorial Passo a Passo

### Passo 1: Criar um Controller Simples

Crie um *API Controller* com endpoints básicos.

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    // GET: api/usuarios
    [HttpGet]
    public IActionResult Listar()
    {
        var usuarios = new[] { new { Id = 1, Nome = "João" }, new { Id = 2, Nome = "Maria" } };
        return Ok(usuarios);
    }

    // GET: api/usuarios/1
    [HttpGet("{id}")]
    public IActionResult Buscar(int id)
    {
        if (id <= 0) return BadRequest("ID inválido");
        return Ok(new { Id = id, Nome = "Usuário " + id });
    }

    // POST: api/usuarios
    [HttpPost]
    public IActionResult Criar([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return CreatedAtAction(nameof(Buscar), new { id = usuario.Id }, usuario);
    }
}

public class UsuarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
}
```

### Passo 2: Configurar no Program.cs

Registre os *controllers* no pipeline.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configura o pipeline
app.UseRouting();
app.MapControllers();

app.Run();
```

### Passo 3: Testar os Endpoints

- **GET /api/usuarios**:
  - Resposta: `200 OK` com `[{"id": 1, "nome": "João"}, {"id": 2, "nome": "Maria"}]`
- **GET /api/usuarios/1**:
  - Resposta: `200 OK` com `{"id": 1, "nome": "Usuário 1"}`
- **GET /api/usuarios/-1**:
  - Resposta: `400 Bad Request` com `"ID inválido"`
- **POST /api/usuarios** com `{ "id": 3, "nome": "Pedro" }`:
  - Resposta: `201 Created` com `{"id": 3, "nome": "Pedro"}` e header `Location: /api/usuarios/3`

---

## Boas Práticas

1. **Use `[ApiController]`**: Habilita funcionalidades como validação automática e retorno consistente de erros.
2. **Separe responsabilidades**: Delegue lógica de negócios a serviços injetados, mantendo o *controller* leve.
3. **Defina rotas claras**: Use `[Route]` para evitar ambiguidades e seguir padrões REST.
4. **Retorne IActionResult**: Prefira tipos como `Ok()`, `BadRequest()`, `NotFound()` para flexibilidade.
5. **Valide entradas**: Sempre cheque `ModelState` ou implemente validação adicional.

---

## Conclusão

*Controllers* no ASP.NET Core são essenciais para gerenciar requisições e respostas, oferecendo uma estrutura robusta para APIs e aplicações MVC. Este tutorial demonstra como criar um *controller* simples, configurá-lo e testá-lo, destacando sua integração com *Model Binding* e respostas HTTP. Com boas práticas, eles podem ser o núcleo de uma aplicação escalável e bem organizada.

