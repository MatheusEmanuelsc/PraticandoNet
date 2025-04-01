

# Tipos de Actions e Retornos no ASP.NET Core 

## Índice
1. [O que são Actions e Seus Tipos de Retorno](#o-que-são-actions-e-seus-tipos-de-retorno)
   - [Tipos de Retorno de Actions](#tipos-de-retorno-de-actions)
   - [IActionResult vs. ActionResult<T>: Por que IActionResult é "Recomendado"?](#iactionresult-vs-actionresultt-por-que-iactionresult-é-recomendado)
   - [Quando Usar Cada Tipo](#quando-usar-cada-tipo)
2. [Gerenciando Retornos com Produces e Outros Atributos](#gerenciando-retornos-com-produces-e-outros-atributos)
   - [Atributos Comuns](#atributos-comuns)
   - [Tutorial Passo a Passo](#tutorial-passo-a-passo)
3. [Boas Práticas](#boas-práticas)
4. [Conclusão](#conclusão)

---

## O que são Actions e Seus Tipos de Retorno

*Actions* são métodos em *Controllers* que processam requisições HTTP e retornam respostas, definidas pelo tipo de retorno.

### Tipos de Retorno de Actions

1. **`IActionResult` (ou `Task<IActionResult>` para assíncronas)**  
   - Interface flexível para retornar qualquer resultado HTTP (ex.: `Ok()`, `NotFound()`).

2. **`ActionResult<T>` (ou `Task<ActionResult<T>>` para assíncronas)**  
   - Tipo genérico que retorna um valor específico (`T`) ou um resultado HTTP.

3. **Tipos Específicos (ex.: `string`, `int`)**  
   - Retorna um valor bruto, assumindo `200 OK`.

4. **`Task<T>`**  
   - Retorna um valor específico assíncrono, sem controle explícito de status HTTP.

5. **`void` (ou `Task`)**  
   - Não retorna conteúdo, geralmente `204 No Content`.

### IActionResult vs. ActionResult<T>: Por que IActionResult é "Recomendado"?

- **Histórico**: Antes do ASP.NET Core 2.1 (quando `ActionResult<T>` foi introduzido), `IActionResult` era o único tipo flexível disponível. Isso criou uma cultura de uso amplo, e muitos tutoriais e códigos legados o reforçam como padrão.
- **Flexibilidade**: `IActionResult` permite retornar qualquer tipo de resultado HTTP sem se comprometer com um tipo específico, o que é útil em cenários onde o retorno pode variar muito (ex.: `Ok`, `Redirect`, `File`).
- **Simplicidade**: Não exige tipagem explícita, o que pode ser mais rápido para protótipos ou APIs simples.
- **Comunidade**: Como foi o padrão por mais tempo, muitos desenvolvedores o consideram "mais seguro" ou "mais idiomático".

**Porém**, `ActionResult<T>` trouxe melhorias:
- **Tipagem forte**: Define o tipo de retorno esperado para sucesso, útil para consumidores da API e ferramentas como Swagger.
- **Menos conversões**: Evita casts manuais de `IActionResult` para um tipo específico.
- **Modernidade**: É o padrão mais recente e recomendado pela Microsoft em documentações atuais para APIs RESTful.

**Conclusão dessa discussão**: `IActionResult` é frequentemente "recomendado" por hábito e simplicidade, mas `ActionResult<T>` é tecnicamente superior para APIs modernas, especialmente com ferramentas de documentação e tipagem estática.

### Quando Usar Cada Tipo

- **`IActionResult`**:
  - **Quando usar**: APIs com retornos variados (ex.: JSON, arquivos, redirecionamentos) ou em projetos legados.
  - **Exemplo**: Retornar um arquivo ou redirecionar além de JSON.

- **`ActionResult<T>`**:
  - **Quando usar**: APIs RESTful modernas onde o tipo de sucesso é previsível e você quer tipagem forte.
  - **Exemplo**: Retornar um modelo específico ou um erro HTTP.

- **Tipos Específicos**:
  - **Quando usar**: Endpoints triviais sem necessidade de status HTTP variados.
  - **Exemplo**: Retornar uma string fixa.

- **`Task<T>`**:
  - **Quando usar**: Ações assíncronas simples com retorno implícito `200 OK`.
  - **Exemplo**: Retornar um valor calculado sem erros.

- **`void`/`Task`**:
  - **Quando usar**: Ações que modificam estado sem resposta (ex.: DELETE).
  - **Exemplo**: Exclusão de um recurso.

---

## Gerenciando Retornos com Produces e Outros Atributos

### Atributos Comuns
- **`[Produces("tipo")]`**: Define o tipo de conteúdo retornado (ex.: `application/json`).
- **`[ProducesResponseType(StatusCode, Type)]`**: Documenta códigos de status e tipos de retorno.
- **`[Consumes("tipo")]`**: Especifica o tipo de conteúdo aceito no corpo da requisição.

### Tutorial Passo a Passo

#### Passo 1: Criar um Controller com Actions e Retornos Gerenciados

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")] // Todas as respostas são JSON
public class UsuariosController : ControllerBase
{
    // GET: api/usuarios/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioModel>> GetAsync(int id)
    {
        if (id <= 0) return BadRequest("Invalid ID");
        await Task.Delay(100); // Simula I/O
        if (id > 10) return NotFound();
        return new UsuarioModel { Id = id, Nome = $"User {id}" };
    }

    // POST: api/usuarios
    [HttpPost]
    [Consumes("application/json")] // Aceita apenas JSON no corpo
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UsuarioModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await Task.Delay(50); // Simula salvamento
        return CreatedAtAction(nameof(GetAsync), new { id = usuario.Id }, usuario);
    }

    // DELETE: api/usuarios/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (id <= 0) return BadRequest("Invalid ID");
        await Task.Delay(30); // Simula deleção
        return NoContent();
    }
}

public class UsuarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
}
```

#### Passo 2: Configurar no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

#### Passo 3: Testar os Endpoints

- **GET /api/usuarios/1**:
  - Resposta: `200 OK` com `{"id": 1, "nome": "User 1"}`
- **GET /api/usuarios/-1**:
  - Resposta: `400 Bad Request` com `"Invalid ID"`
- **GET /api/usuarios/11**:
  - Resposta: `404 NotFound`
- **POST /api/usuarios** com `{ "id": 3, "nome": "Pedro" }`:
  - Resposta: `201 Created` com `{"id": 3, "nome": "Pedro"}` e `Location: /api/usuarios/3`
- **DELETE /api/usuarios/1**:
  - Resposta: `204 No Content`

---

## Boas Práticas

1. **Escolha IActionResult para flexibilidade**: Use em cenários com retornos variados ou incertos.
2. **Prefira ActionResult<T> para tipagem**: Ideal para APIs RESTful modernas com Swagger.
3. **Use async para I/O**: Sempre em ações com operações assíncronas.
4. **Documente com Produces**: Torne os retornos explícitos para consumidores e ferramentas.
5. **Consistência**: Padronize o tipo de retorno em toda a API (ex.: sempre JSON com `[Produces]`).

---

## Conclusão

`IActionResult` é amplamente "recomendado" por sua flexibilidade e histórico, mas `ActionResult<T>` é a escolha moderna para APIs tipadas e documentadas. Este tutorial esclarece as opções, mostrando como usar cada tipo e gerenciar retornos com atributos como `[Produces]`. A decisão depende do contexto: `IActionResult` para máxima flexibilidade, `ActionResult<T>` para consistência e tipagem.

