

# Versão Nova (Atualizada e Moderna)

## Actions PATCH no ASP.NET Core

### Pacote Necessário (Versão Nova)
Para suportar JSON Patch com Newtonsoft.Json, o único pacote necessário é:
```
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
```

---

### Tutorial Passo a Passo

#### Passo 1: Instalar Pacote Necessário
(Conforme listado acima: `Microsoft.AspNetCore.Mvc.NewtonsoftJson`).

#### Passo 2: Configurar o Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Adiciona controllers com suporte a Newtonsoft.Json para JSON Patch
builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();
```

#### Passo 3: Criar o Controller com Action PATCH
```csharp
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.JsonPatch;

[ApiController]
[Route("api/[controller]")]
public class TarefasController : ControllerBase
{
    private readonly IMeuRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TarefasController(IMeuRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPatch("{id}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync(int id, [FromBody] JsonPatchDocument<TarefaPatchDto>? patchDoc)
    {
        if (patchDoc == null) return BadRequest("Documento de patch inválido");

        var tarefaEntity = await _repository.GetTarefaByIdAsync(id);
        if (tarefaEntity == null) return NotFound();

        var tarefaToPatch = _mapper.Map<TarefaPatchDto>(tarefaEntity);
        patchDoc.ApplyTo(tarefaToPatch, error => ModelState.AddModelError("", error.ErrorMessage));
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        _mapper.Map(tarefaToPatch, tarefaEntity);
        _repository.UpdateTarefa(tarefaEntity);
        await _unitOfWork.CommitAsync();

        return NoContent();
    }
}

// DTO (assumindo que você já o tem configurado com FluentValidation)
public class TarefaPatchDto
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public bool? Concluida { get; set; }
}
```

#### Passo 4: Testar o Endpoint
- **PATCH /api/tarefas/1**:
  ```json
  [
      { "op": "replace", "path": "/titulo", "value": "Tarefa Atualizada" }
  ]
  ```
  - Resposta: `204 No Content`

---

### Conclusão
A versão nova usa `Microsoft.AspNetCore.Mvc.NewtonsoftJson` para suportar JSON Patch de forma robusta, integrando-se bem com AutoMapper e FluentValidation que você já possui.

---

# Versão Antiga (Fornecida por Você)

## Actions PATCH no ASP.NET Core

### Pacote Necessário (Versão Antiga)
Para suportar JSON Patch na versão antiga, o único pacote necessário é:
```
dotnet add package Microsoft.AspNetCore.JsonPatch
```

---

### Tutorial Passo a Passo

#### Passo 1: Instalar Pacote Necessário
(Conforme listado acima: `Microsoft.AspNetCore.JsonPatch`).

#### Passo 2: Criar o Controller com Action PATCH
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly List<UsuarioModel> _usuarios = new()
    {
        new() { Id = 1, Nome = "João", Pedidos = new() { new() { Id = 101, Produto = "Livro" } } }
    };

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

        patchDoc.ApplyTo(usuario, ModelState);
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
```

#### Passo 3: Configurar no Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

#### Passo 4: Testar o Endpoint
- **PATCH /api/usuarios/1**:
  ```json
  [
      { "op": "replace", "path": "/nome", "value": "João Atualizado" }
  ]
  ```
  - Resposta: `200 OK` com `{"id": 1, "nome": "João Atualizado", "pedidos": [{"id": 101, "produto": "Livro"}]}`

---

### Conclusão
A versão antiga usa `Microsoft.AspNetCore.JsonPatch` com System.Text.Json nativo, sem integração com FluentValidation ou AutoMapper, sendo mais simples e menos robusta.

---

# Diferenças Chave
- **Pacote**: Nova usa `Microsoft.AspNetCore.Mvc.NewtonsoftJson` (mais robusto); Antiga usa `Microsoft.AspNetCore.JsonPatch` (nativo, mas menos flexível).
- **Integração**: Nova assume uso de AutoMapper e FluentValidation (que você já tem); Antiga não os utiliza.
- **Retorno**: Nova retorna `204 No Content` (RESTful); Antiga retorna `200 OK` com o recurso.

