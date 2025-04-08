

```md
# PATCH no ASP.NET Core 8 com JsonPatchDocument, AutoMapper e FluentValidation

## 📦 Pacote Recomendado (Moderno e Mantido)
Para utilizar `JsonPatchDocument` com suporte robusto, use:

```bash
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
```

> ❌ **Evite o pacote `Microsoft.AspNetCore.JsonPatch`**, que é legado, não mais mantido e aparece como **obsoleto** no Rider.  
> ✅ A abordagem moderna usa `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, com suporte completo a validação, AutoMapper e JSON Patch.

---

## ⚙️ Configuração no `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(); // suporte ao JSON Patch com Newtonsoft

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## ✅ Exemplo de `TarefaPatchDto`

```csharp
public class TarefaPatchDto
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public bool? Concluida { get; set; }
}
```

---

## ✅ Validação Parcial com FluentValidation

```csharp
public class TarefaPatchValidator : AbstractValidator<TarefaPatchDto>
{
    public TarefaPatchValidator()
    {
        RuleFor(x => x.Titulo)
            .MaximumLength(100)
            .When(x => x.Titulo is not null);

        RuleFor(x => x.Concluida)
            .NotNull()
            .When(x => x.Concluida.HasValue);
    }
}
```

> 🧠 **Por que usar `When`?**  
> Em operações PATCH, os campos são opcionais. A validação deve acontecer **somente se o campo for incluído no JSON Patch**, por isso usamos `.When(...)` para validar **condicionalmente**.

---

## 🚀 Exemplo de Controller com PATCH

```csharp
[HttpPatch("{id}")]
[Consumes("application/json-patch+json")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> PatchAsync(int id, [FromBody] JsonPatchDocument<TarefaPatchDto>? patchDoc)
{
    if (patchDoc == null)
        return BadRequest("Documento de patch inválido");

    var entity = await _repository.GetTarefaByIdAsync(id);
    if (entity == null)
        return NotFound();

    var dtoToPatch = _mapper.Map<TarefaPatchDto>(entity);

    patchDoc.ApplyTo(dtoToPatch, ModelState);

    if (!TryValidateModel(dtoToPatch) || !ModelState.IsValid)
        return ValidationProblem(ModelState);

    _mapper.Map(dtoToPatch, entity);
    _repository.UpdateTarefa(entity);
    await _unitOfWork.CommitAsync();

    return NoContent();
}
```

---

## 🧪 Exemplo de Requisição PATCH

**Requisição:**
```http
PATCH /api/tarefas/1
Content-Type: application/json-patch+json
```

**Corpo:**
```json
[
  { "op": "replace", "path": "/titulo", "value": "Novo título" }
]
```

**Resposta:**  
```http
204 No Content
```

---

## ✅ Boas Práticas

| Boa Prática                              | Descrição |
|------------------------------------------|-----------|
| ✅ Validar apenas campos alterados       | Use `.When(...)` no `FluentValidation` para validar somente os campos presentes |
| ✅ Usar `TryValidateModel(dtoToPatch)`   | Garante que a validação do DTO ocorra após aplicação do patch |
| ✅ Usar `JsonPatchDocument<TDto>`        | Nunca aplique o patch diretamente sobre a entidade |
| ✅ Retornar `204 No Content`             | RESTful e sem necessidade de retornar o recurso atualizado |
| ❌ Evite `Microsoft.AspNetCore.JsonPatch`| Pacote legado e obsoleto |

---

## 🔍 Comparativo com Abordagem Antiga

| Item                          | Versão Moderna                               | Versão Antiga                           |
|-------------------------------|-----------------------------------------------|------------------------------------------|
| Pacote                        | `Microsoft.AspNetCore.Mvc.NewtonsoftJson`     | `Microsoft.AspNetCore.JsonPatch` (obsoleto) |
| Integra com FluentValidation | ✅ Sim                                         | ❌ Não                                   |
| Suporte ao AutoMapper        | ✅ Sim                                         | ⚠️ Manual                                |
| Suporte oficial               | ✅ Mantido                                     | ❌ Descontinuado                         |

---

## 🧾 Conclusão

A implementação de PATCH no ASP.NET Core 8 deve seguir a abordagem moderna com:

- `JsonPatchDocument<TDto>`  
- AutoMapper para projeções  
- FluentValidation com regras condicionais  
- Validação pós-patch com `TryValidateModel`  
- E o pacote **`Microsoft.AspNetCore.Mvc.NewtonsoftJson`**, que é o **único oficialmente mantido** e recomendado.

---
