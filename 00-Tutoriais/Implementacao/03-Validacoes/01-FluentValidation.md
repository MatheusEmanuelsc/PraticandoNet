# FluentValidation no .NET 8

## Índice

1. [Introdução](#introducao)
2. [Instalação](#instalacao)
3. [Criando um Validador](#criando-um-validador)
4. [Validando Dados](#validando-dados)
5. [Integração com ASP.NET Core](#integracao-com-aspnet-core)
6. [Principais Métodos do FluentValidation](#principais-metodos)
7. [Conclusão](#conclusao)

---

## 1. Introdução {#introducao}

O FluentValidation é uma biblioteca para .NET que facilita a definição de regras de validação para modelos de dados de forma clara e fluente. Ele ajuda a separar a lógica de validação da lógica de negócio, promovendo um código mais limpo e organizado.

## 2. Instalação {#instalacao}

Para instalar o FluentValidation em seu projeto .NET 8, execute o seguinte comando:

```bash
    dotnet add package FluentValidation.AspNetCore
```

## 3. Criando um Validador {#criando-um-validador}

Vamos criar um exemplo de modelo e seu respectivo validador.

```csharp
using FluentValidation;

public class Pessoa
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public int Idade { get; set; }
}

public class PessoaValidator : AbstractValidator<Pessoa>
{
    public PessoaValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O Nome é obrigatório")
            .Length(2, 100).WithMessage("O Nome deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O Email é obrigatório")
            .EmailAddress().WithMessage("O Email deve ser válido");

        RuleFor(x => x.Idade)
            .GreaterThan(0).WithMessage("A idade deve ser maior que zero");
    }
}
```

## 4. Validando Dados {#validando-dados}

Podemos validar os dados de uma instância da classe `Pessoa` da seguinte maneira:

```csharp
var pessoa = new Pessoa { Nome = "", Email = "emailinvalido", Idade = -1 };
var validator = new PessoaValidator();
var resultado = validator.Validate(pessoa);

if (!resultado.IsValid)
{
    foreach (var erro in resultado.Errors)
    {
        Console.WriteLine($"Erro na propriedade {erro.PropertyName}: {erro.ErrorMessage}");
    }
}
```

## 5. Integração com ASP.NET Core {#integracao-com-aspnet-core}

Para integrar o FluentValidation com ASP.NET Core, registre os validadores no contêiner de injeção de dependência:

```csharp
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<PessoaValidator>();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

Em um controlador, a validação ocorre automaticamente:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PessoaController : ControllerBase
{
    [HttpPost]
    public IActionResult Criar([FromBody] Pessoa pessoa)
    {
        return Ok("Pessoa válida!");
    }
}
```

## 6. Principais Métodos do FluentValidation {#principais-metodos}

| Método | Descrição |
|---------|------------|
| `NotEmpty()` | Garante que o valor não seja nulo ou vazio. |
| `NotNull()` | Garante que o valor não seja nulo. |
| `Length(min, max)` | Define um intervalo de tamanho permitido. |
| `EmailAddress()` | Valida se o campo é um email válido. |
| `GreaterThan(valor)` | O valor deve ser maior que o especificado. |
| `LessThan(valor)` | O valor deve ser menor que o especificado. |
| `Matches(regex)` | Valida o campo com uma expressão regular. |
| `InclusiveBetween(min, max)` | Define um intervalo de valores permitidos, incluindo os extremos. |
| `WithMessage(string)` | Define uma mensagem personalizada para o erro. |

## 7. Conclusão {#conclusao}

O FluentValidation facilita a implementação de validações em aplicações .NET de maneira fluida e reutilizável. Integrando-o corretamente, podemos garantir a qualidade dos dados sem poluir nossas classes de negócio com lógica de validação.

