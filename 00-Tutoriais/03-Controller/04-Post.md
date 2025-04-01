

# Actions POST no ASP.NET Core

## Índice
1. [O que são Actions POST?](#o-que-são-actions-post)
2. [Cenários de Uso para POST](#cenários-de-uso-para-post)
   - [Criar um Recurso](#criar-um-recurso)
   - [Criar e Retornar com GET](#criar-e-retornar-com-get)
   - [Outros Casos (Validação, Relacionamentos)](#outros-casos-validação-relacionamentos)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Controller com Actions POST e GET](#passo-1-criar-o-controller-com-actions-post-e-get)
   - [Passo 2: Configurar no Program.cs](#passo-2-configurar-no-programcs)
   - [Passo 3: Testar os Endpoints](#passo-3-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Actions POST?

*Actions* POST são métodos em *Controllers* que lidam com requisições HTTP POST, usadas para criar novos recursos no servidor. Elas geralmente recebem dados no corpo da requisição, validam-nos e retornam uma resposta indicando sucesso (ex.: `201 Created`) ou erro.

---

## Cenários de Uso para POST

### Criar um Recurso
- **Descrição**: Recebe dados, cria o recurso e retorna o recurso criado ou apenas um status.
- **Recomendações**: Use `async` para operações de I/O (ex.: salvar no banco) e `[FromBody]` para o payload.

### Criar e Retornar com GET
- **Descrição**: Após criar o recurso, chama internamente uma *Action* GET para retornar os detalhes completos do recurso criado.
- **Recomendações**: Use `CreatedAtAction` para gerar a URI e garantir consistência com o GET existente.

### Outros Casos (Validação, Relacionamentos)
- **Descrição**: Inclui validação de entrada ou criação com relacionamentos (ex.: usuário e pedidos).
- **Recomendações**: Valide com `ModelState` e use `async` para salvar dados relacionados.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Controller com Actions POST e GET

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
    private readonly List<UsuarioModel> _usuarios = new();

    // GET: api/usuarios/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        await Task.Delay(50); // Simula I/O
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // POST: api/usuarios - Criar e chamar GET
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Simula criação assíncrona
        usuario.Id = _usuarios.Count + 1; // Gera ID fictício
        await Task.Delay(50); // Simula salvamento
        _usuarios.Add(usuario);

        // Retorna com chamada ao GET
        return CreatedAtAction(nameof(GetByIdAsync), new { id = usuario.Id }, usuario);
    }

    // POST: api/usuarios/completo - Criar com relacionamentos
    [HttpPost("completo")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCompleteAsync([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid || usuario.Pedidos == null || !usuario.Pedidos.Any())
            return BadRequest(ModelState);

        // Simula criação com relacionamentos
        usuario.Id = _usuarios.Count + 1;
        await Task.Delay(50); // Simula salvamento
        _usuarios.Add(usuario);

        // Retorna com chamada ao GET
        return CreatedAtAction(nameof(GetByIdAsync), new { id = usuario.Id }, usuario);
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

- **POST /api/usuarios** com `{ "nome": "João" }`:
  - Resposta: `201 Created` com `{"id": 1, "nome": "João", "pedidos": null}` e `Location: /api/usuarios/1`
  - **GET /api/usuarios/1**: `200 OK` com `{"id": 1, "nome": "João", "pedidos": null}`

- **POST /api/usuarios/completo** com `{ "nome": "Maria", "pedidos": [{"id": 101, "produto": "Livro"}] }`:
  - Resposta: `201 Created` com `{"id": 2, "nome": "Maria", "pedidos": [{"id": 101, "produto": "Livro"}]}` e `Location: /api/usuarios/2`
  - **GET /api/usuarios/2**: `200 OK` com o mesmo conteúdo

- **POST /api/usuarios** com `{ "nome": "" }` (inválido):
  - Resposta: `400 Bad Request` com erros do `ModelState`

---

## Boas Práticas

1. **Use async para I/O**: Sempre aplique `async Task` em operações de salvamento.
2. **Valide antes de criar**: Cheque `ModelState` para garantir dados válidos.
3. **Retorne 201 com Location**: Use `CreatedAtAction` para conformidade RESTful e consistência com GET.
4. **Inclua relacionamentos com cuidado**: Valide dados relacionados e evite sobrecarga desnecessária.
5. **Documente retornos**: Use `[ProducesResponseType]` para clareza em Swagger.

---

## Conclusão

As *Actions* POST no ASP.NET Core são ideais para criar recursos, e integrar o retorno com uma chamada ao GET (via `CreatedAtAction`) garante que o cliente receba o recurso recém-criado de forma consistente. Este tutorial cobre a criação simples, com relacionamentos e validação, usando `async/await` para simular operações de I/O. É uma abordagem prática e alinhada com padrões REST.

