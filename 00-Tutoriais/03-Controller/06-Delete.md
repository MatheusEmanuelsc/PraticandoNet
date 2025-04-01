

# Actions DELETE no ASP.NET Core

## Índice
1. [O que são Actions DELETE?](#o-que-são-actions-delete)
2. [Cenários de Uso para DELETE](#cenários-de-uso-para-delete)
   - [Excluir um Recurso por ID](#excluir-um-recurso-por-id)
   - [Excluir com Relacionamentos](#excluir-com-relacionamentos)
   - [Outros Casos (Validação, Recurso Não Encontrado)](#outros-casos-validação-recurso-não-encontrado)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Controller com Actions DELETE](#passo-1-criar-o-controller-com-actions-delete)
   - [Passo 2: Configurar no Program.cs](#passo-2-configurar-no-programcs)
   - [Passo 3: Testar os Endpoints](#passo-3-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Actions DELETE?

*Actions* DELETE são métodos em *Controllers* que lidam com requisições HTTP DELETE, usadas para remover recursos existentes no servidor. Elas geralmente retornam `204 No Content` em caso de sucesso ou `404 Not Found` se o recurso não existe.

---

## Cenários de Uso para DELETE

### Excluir um Recurso por ID
- **Descrição**: Remove um recurso específico identificado por um ID.
- **Recomendações**: Use `async` para operações de I/O e `[FromRoute]` para o ID.

### Excluir com Relacionamentos
- **Descrição**: Remove um recurso e seus dados relacionados (ex.: usuário e seus pedidos).
- **Recomendações**: Use `async` para exclusões complexas e valide a existência do recurso principal.

### Outros Casos (Validação, Recurso Não Encontrado)
- **Descrição**: Trata validação de entrada (ex.: ID inválido) ou casos onde o recurso não existe.
- **Recomendações**: Retorne `400 Bad Request` para IDs inválidos e `404 Not Found` se o recurso não for encontrado.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Controller com Actions DELETE

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

    // DELETE: api/usuarios/1 - Excluir recurso por ID
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        if (id <= 0) return BadRequest("ID inválido");

        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        // Simula exclusão assíncrona
        await Task.Delay(50);
        _usuarios.Remove(usuario);

        return NoContent();
    }

    // DELETE: api/usuarios/1/pedidos - Excluir apenas relacionamentos
    [HttpDelete("{id}/pedidos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePedidosAsync([FromRoute] int id)
    {
        if (id <= 0) return BadRequest("ID inválido");

        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        if (usuario.Pedidos == null || !usuario.Pedidos.Any())
            return BadRequest("Nenhum pedido para excluir");

        // Simula exclusão assíncrona dos relacionamentos
        await Task.Delay(50);
        usuario.Pedidos.Clear();

        return NoContent();
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

- **DELETE /api/usuarios/1**:
  - Resposta: `204 No Content` (usuário com ID 1 removido)
  - Após exclusão, a lista contém apenas o usuário com ID 2
- **DELETE /api/usuarios/999**:
  - Resposta: `404 Not Found`
- **DELETE /api/usuarios/-1**:
  - Resposta: `400 Bad Request` com `"ID inválido"`

- **DELETE /api/usuarios/2/pedidos**:
  - Resposta: `204 No Content` (pedidos do usuário 2 removidos)
  - O usuário 2 agora tem `pedidos: []`
- **DELETE /api/usuarios/2/pedidos** (após esvaziar):
  - Resposta: `400 Bad Request` com `"Nenhum pedido para excluir"`

---

## Boas Práticas

1. **Use async para I/O**: Aplique `async Task` em operações de exclusão no banco.
2. **Valide o ID**: Retorne `400` para IDs inválidos e `404` para recursos inexistentes.
3. **Retorne 204**: Use `NoContent()` para indicar sucesso sem necessidade de corpo na resposta.
4. **Trate relacionamentos**: Considere exclusão em cascata ou específica, conforme o caso.
5. **Documente retornos**: Use `[ProducesResponseType]` para clareza em Swagger.

---

## Conclusão

As *Actions* DELETE no ASP.NET Core são projetadas para remover recursos de forma eficiente, seja o recurso principal ou seus relacionamentos. Este tutorial cobre a exclusão por ID e de dados relacionados, usando `async/await` para simular operações de I/O e seguindo padrões RESTful com retornos apropriados (`204`, `404`, `400`). É uma abordagem simples e robusta para gerenciar exclusões.

