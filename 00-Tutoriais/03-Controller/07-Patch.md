

# Actions PATCH no ASP.NET Core

## Índice
1. [O que são Actions PATCH?](#o-que-são-actions-patch)
2. [Cenários de Uso para PATCH](#cenários-de-uso-para-patch)
   - [Atualização Parcial com JSON Patch](#atualização-parcial-com-json-patch)
   - [Atualização Parcial Simples](#atualização-parcial-simples)
   - [Outros Casos (Validação, Relacionamentos)](#outros-casos-validação-relacionamentos)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Instalar Pacotes Necessários](#passo-1-instalar-pacotes-necessários)
   - [Passo 2: Criar o Controller com Actions PATCH](#passo-2-criar-o-controller-com-actions-patch)
   - [Passo 3: Configurar no Program.cs](#passo-3-configurar-no-programcs)
   - [Passo 4: Testar os Endpoints](#passo-4-testar-os-endpoints)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que são Actions PATCH?

*Actions* PATCH são métodos em *Controllers* que lidam com requisições HTTP PATCH, usadas para atualizar parcialmente recursos existentes no servidor. Diferente do PUT, que substitui o recurso inteiro, o PATCH modifica apenas os campos especificados, retornando geralmente `200 OK` ou `204 No Content`.

---

## Cenários de Uso para PATCH

### Atualização Parcial com JSON Patch
- **Descrição**: Usa o formato JSON Patch (RFC 6902) para aplicar operações específicas (ex.: substituir, adicionar) em um recurso.
- **Recomendações**: Use `async` para I/O e a biblioteca `Microsoft.AspNetCore.JsonPatch`.

### Atualização Parcial Simples
- **Descrição**: Atualiza campos específicos fornecidos no corpo da requisição, sem seguir um padrão como JSON Patch.
- **Recomendações**: Use `async` para salvamento e valide os campos enviados.

### Outros Casos (Validação, Relacionamentos)
- **Descrição**: Trata validação de entrada ou atualizações parciais em relacionamentos.
- **Recomendações**: Retorne `400 Bad Request` para dados inválidos e `404 Not Found` se o recurso não existir.

---

## Tutorial Passo a Passo

### Passo 1: Instalar Pacotes Necessários

Adicione o pacote para suportar JSON Patch:
```
dotnet add package Microsoft.AspNetCore.JsonPatch
```

### Passo 2: Criar o Controller com Actions PATCH

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
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

    // PATCH: api/usuarios/1 - Atualização parcial com JSON Patch
    [HttpPatch("{id}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UsuarioModel> patchDoc)
    {
        if (patchDoc == null) return BadRequest("Documento de patch inválido");

        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        // Aplica o patch
        patchDoc.ApplyTo(usuario, ModelState);
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Simula salvamento assíncrono
        await Task.Delay(50);
        return Ok(usuario);
    }

    // PATCH: api/usuarios/1/simples - Atualização parcial simples
    [HttpPatch("{id}/simples")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchSimpleAsync([FromRoute] int id, [FromBody] UsuarioPatchModel patch)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        // Atualiza apenas os campos fornecidos
        if (!string.IsNullOrEmpty(patch.Nome)) usuario.Nome = patch.Nome;
        if (patch.Pedidos != null) usuario.Pedidos = patch.Pedidos;

        // Simula salvamento assíncrono
        await Task.Delay(50);
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

public class UsuarioPatchModel
{
    public string? Nome { get; set; }
    public List<PedidoModel>? Pedidos { get; set; }
}
```

### Passo 3: Configurar no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

### Passo 4: Testar os Endpoints

- **PATCH /api/usuarios/1** com JSON Patch:
  ```json
  [
      { "op": "replace", "path": "/nome", "value": "João Atualizado" },
      { "op": "replace", "path": "/pedidos", "value": [{"id": 102, "produto": "Caneta"}] }
  ]
  ```
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Atualizado", "pedidos": [{"id": 102, "produto": "Caneta"}]}`
- **PATCH /api/usuarios/999** (não existente):
  - Resposta: `404 Not Found`
- **PATCH /api/usuarios/1** com JSON inválido (ex.: `"op": "invalid"`):
  - Resposta: `400 Bad Request`

- **PATCH /api/usuarios/1/simples** com `{ "nome": "João Simples" }`:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Simples", "pedidos": [{"id": 102, "produto": "Caneta"}]}`
- **PATCH /api/usuarios/1/simples** com `{ "pedidos": [{"id": 103, "produto": "Caderno"}] }`:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Simples", "pedidos": [{"id": 103, "produto": "Caderno"}]}`

---

## Boas Práticas

1. **Use async para I/O**: Aplique `async Task` em operações de salvamento no banco.
2. **Prefira JSON Patch para padrão**: Use o formato JSON Patch para conformidade RESTful em atualizações parciais.
3. **Valide o patch**: Verifique `ModelState` após aplicar o patch ou ao usar modelos simples.
4. **Retorne o recurso atualizado**: Use `200 OK` com o recurso ou `204 No Content` se preferir não retornar dados.
5. **Documente retornos**: Use `[ProducesResponseType]` e `[Consumes]` para clareza em Swagger.

---

## Conclusão

As *Actions* PATCH no ASP.NET Core são ideais para atualizações parciais, oferecendo flexibilidade com JSON Patch ou modelos simples. Este tutorial cobre ambos os cenários, usando `async/await` para simular operações de I/O e garantindo conformidade RESTful com validação e retornos apropriados. JSON Patch é recomendado para APIs padronizadas, enquanto a abordagem simples é útil para casos menos formais.

