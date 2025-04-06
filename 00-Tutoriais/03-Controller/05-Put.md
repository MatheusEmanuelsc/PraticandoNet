

# Actions PUT no ASP.NET Core

## Índice
1. [O que são Actions PUT?](#o-que-são-actions-put)
2. [Cenários de Uso para PUT](#cenários-de-uso-para-put)
   - [Atualizar um Recurso Completo](#atualizar-um-recurso-completo)
   - [Atualizar com Relacionamentos](#atualizar-com-relacionamentos)
   - [Outros Casos (Validação, Recurso Não Encontrado)](#outros-casos-validação-recurso-não-encontrado)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Controller com Actions PUT](#passo-1-criar-o-controller-com-actions-put)
   - [Passo 2: Configurar no Program.cs](#passo-2-configurar-no-programcs)
   - [Passo 3: Testar os Endpoints](#passo-3-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Actions PUT?

*Actions* PUT são métodos em *Controllers* que lidam com requisições HTTP PUT, usadas para atualizar recursos existentes no servidor. Elas substituem completamente o recurso identificado pela URI com os dados fornecidos, retornando geralmente `200 OK` ou `204 No Content`.

---

## Cenários de Uso para PUT

### Atualizar um Recurso Completo
- **Descrição**: Atualiza todos os campos de um recurso com base no ID fornecido.
- **Recomendações**: Use `async` para operações de I/O e `[FromBody]` para o payload.

### Atualizar com Relacionamentos
- **Descrição**: Atualiza o recurso principal e seus dados relacionados (ex.: usuário e pedidos).
- **Recomendações**: Valide relacionamentos e use `async` para salvar as alterações.

### Outros Casos (Validação, Recurso Não Encontrado)
- **Descrição**: Trata validação de entrada ou casos onde o recurso não existe.
- **Recomendações**: Retorne `400 Bad Request` para dados inválidos e `404 Not Found` se o recurso não for encontrado.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Controller com Actions PUT

```csharp
 impeachment Microsoft.AspNetCore.Mvc;
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
        new() { Id = 1, Nome = "João", Pedidos = new() { new() { Id = 101, Produto = "Livro" } } }
    };

    // PUT: api/usuarios/1 - Atualizar recurso completo
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid || id != usuario.Id) 
            return BadRequest("Dados inválidos ou ID inconsistente");

        var existente = _usuarios.FirstOrDefault(u => u.Id == id);
        if (existente == null) return NotFound();

        // Simula atualização assíncrona
        await Task.Delay(50);
        existente.Nome = usuario.Nome;
        existente.Pedidos = usuario.Pedidos;

        return Ok(existente);
    }

    // PUT: api/usuarios/1/pedidos - Atualizar apenas relacionamentos
    [HttpPut("{id}/pedidos")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePedidosAsync([FromRoute] int id, [FromBody] List<PedidoModel> pedidos)
    {
        if (!ModelState.IsValid || !pedidos.Any()) 
            return BadRequest("Lista de pedidos inválida");

        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        // Simula atualização assíncrona
        await Task.Delay(50);
        usuario.Pedidos = pedidos;

        return Ok(usuario);
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

- **PUT /api/usuarios/1** com `{ "id": 1, "nome": "João Atualizado", "pedidos": [{"id": 102, "produto": "Caneta"}] }`:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Atualizado", "pedidos": [{"id": 102, "produto": "Caneta"}]}`
- **PUT /api/usuarios/999** com `{ "id": 999, "nome": "Novo" }`:
  - Resposta: `404 Not Found`
- **PUT /api/usuarios/1** com `{ "id": 2, "nome": "Erro" }` (ID inconsistente):
  - Resposta: `400 Bad Request` com `"Dados inválidos ou ID inconsistente"`

- **PUT /api/usuarios/1/pedidos** com `[{"id": 103, "produto": "Caderno"}]`:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Atualizado", "pedidos": [{"id": 103, "produto": "Caderno"}]}`
- **PUT /api/usuarios/1/pedidos** com `[]` (lista vazia):
  - Resposta: `400 Bad Request` com `"Lista de pedidos inválida"`

---

## Boas Práticas

1. **Use async para I/O**: Sempre aplique `async Task` em operações de atualização no banco.
2. **Valide o ID**: Garanta que o ID da rota e do corpo sejam consistentes.
3. **Retorne o recurso atualizado**: Use `200 OK` com o recurso ou `204 No Content` se preferir não retornar dados.
4. **Trate não encontrados**: Retorne `404` para IDs inexistentes.
5. **Documente com Produces**: Especifique tipos de retorno para clareza em Swagger.

---

## Conclusão

As *Actions* PUT no ASP.NET Core são ideais para atualizar recursos existentes, seja completamente ou em partes específicas (ex.: relacionamentos). Este tutorial cobre a atualização de um recurso inteiro e de seus relacionamentos, usando `async/await` para simular operações de I/O e garantindo conformidade RESTful com validação e retornos apropriados. O padrão é substituir o recurso, então todos os dados enviados devem ser refletidos na atualização.



