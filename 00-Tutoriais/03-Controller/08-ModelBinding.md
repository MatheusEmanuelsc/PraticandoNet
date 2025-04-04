

# Model Binding no ASP.NET Core

## Índice
1. [O que é Model Binding?](#o-que-é-model-binding)
2. [Como Funciona no ASP.NET Core?](#como-funciona-no-aspnet-core)
   - [Fontes de Dados](#fontes-de-dados)
   - [Atributos Comuns](#atributos-comuns)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Modelo](#passo-1-criar-o-modelo)
   - [Passo 2: Configurar o Endpoint](#passo-2-configurar-o-endpoint)
   - [Passo 3: Testar o Binding](#passo-3-testar-o-binding)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que é Model Binding?

*Model Binding* é o processo pelo qual o ASP.NET Core mapeia automaticamente dados de uma requisição HTTP (como query strings, formulários, JSON, etc.) para parâmetros ou propriedades de modelos em ações de controladores. Ele simplifica o acesso a dados enviados pelo cliente.

---

## Como Funciona no ASP.NET Core?

O *Model Binding* é um recurso nativo que utiliza provedores de binding para extrair dados de diferentes partes da requisição e associá-los a objetos ou variáveis.

### Fontes de Dados
- **Query String**: `?nome=João&idade=25`
- **Route Data**: `/usuarios/1`
- **Formulário**: Dados de `<form>` em POST
- **Corpo da Requisição**: JSON ou XML em POST/PUT
- **Cabeçalhos**: Headers HTTP

### Atributos Comuns
- `[FromQuery]`: Liga a uma query string.
- `[FromRoute]`: Liga a parâmetros de rota.
- `[FromBody]`: Liga ao corpo da requisição (ex.: JSON).
- `[FromForm]`: Liga a dados de formulário.
- `[BindRequired]`: Torna um campo obrigatório.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Modelo

Crie uma classe para representar os dados que serão recebidos.

```csharp
public class UsuarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int Idade { get; set; }
}
```

### Passo 2: Configurar o Endpoint

Adicione um controlador com ações que utilizam *Model Binding*.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    // Binding via query string
    [HttpGet("buscar")]
    public IActionResult BuscarPorQuery([FromQuery] string nome, [FromQuery] int idade)
    {
        return Ok($"Nome: {nome}, Idade: {idade}");
    }

    // Binding via rota
    [HttpGet("{id}")]
    public IActionResult BuscarPorId([FromRoute] int id)
    {
        return Ok($"ID: {id}");
    }

    // Binding via corpo (JSON)
    [HttpPost]
    public IActionResult Criar([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        return Ok(usuario);
    }

    // Binding via formulário
    [HttpPost("form")]
    public IActionResult CriarPorForm([FromForm] UsuarioModel usuario)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        return Ok(usuario);
    }
}
```

### Passo 3: Testar o Binding

No `Program.cs`, configure os controladores e teste os endpoints.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

- **Teste Query String**: `GET /api/usuarios/buscar?nome=João&idade=25`
  - Resposta: `Nome: João, Idade: 25`
- **Teste Rota**: `GET /api/usuarios/42`
  - Resposta: `ID: 42`
- **Teste JSON**: `POST /api/usuarios` com corpo `{ "id": 1, "nome": "Maria", "idade": 30 }`
  - Resposta: `{ "id": 1, "nome": "Maria", "idade": 30 }`
- **Teste Formulário**: `POST /api/usuarios/form` com `multipart/form-data` (ex.: `Id=2&Nome=Pedro&Idade=22`)
  - Resposta: `{ "id": 2, "nome": "Pedro", "idade": 22 }`

---

## Boas Práticas

1. **Valide o ModelState**: Sempre verifique `ModelState.IsValid` para garantir que os dados estão corretos.
2. **Use atributos específicos**: Prefira `[FromQuery]`, `[FromBody]`, etc., para clareza e controle.
3. **Evite sobrecarga**: Não misture muitas fontes de binding em uma única ação, exceto quando necessário.
4. **Defina tipos adequados**: Use modelos fortemente tipados em vez de parâmetros soltos para ações complexas.
5. **Personalize erros**: Retorne respostas úteis quando o binding falhar (ex.: `BadRequest(ModelState)`).

---

## Conclusão

O *Model Binding* no ASP.NET Core é uma ferramenta poderosa e nativa que simplifica o mapeamento de dados de requisições para objetos ou parâmetros. Ele suporta várias fontes de dados e é altamente configurável com atributos, eliminando a necessidade de parsing manual. Com este tutorial, você pode implementar e testar rapidamente o *Model Binding* em diferentes cenários.

