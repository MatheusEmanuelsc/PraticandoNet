

# Validação Fluente no ASP.NET Core com FluentValidation

## Índice
1. [O que é Validação Fluente?](#o-que-é-validação-fluente)
2. [FluentValidation no ASP.NET Core](#fluentvalidation-no-aspnet-core)
   - [Por que Usar FluentValidation?](#por-que-usar-fluentvalidation)
   - [Integração com Model Binding](#integração-com-model-binding)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Instalar o Pacote](#passo-1-instalar-o-pacote)
   - [Passo 2: Criar o Modelo](#passo-2-criar-o-modelo)
   - [Passo 3: Definir o Validador](#passo-3-definir-o-validador)
   - [Passo 4: Configurar no Program.cs](#passo-4-configurar-no-programcs)
   - [Passo 5: Configurar o Endpoint](#passo-5-configurar-o-endpoint)
   - [Passo 6: Testar a Validação](#passo-6-testar-a-validação)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que é Validação Fluente?

Validação fluente é uma abordagem que utiliza uma API encadeável (*fluent*) para definir regras de validação de forma clara e expressiva, geralmente separando as regras do modelo. No ASP.NET Core, a biblioteca *FluentValidation* é a mais popular para isso.

---

## FluentValidation no ASP.NET Core

### Por que Usar FluentValidation?
- **Separação de responsabilidades**: Regras de validação ficam fora do modelo.
- **Flexibilidade**: Suporta validações complexas (ex.: condicionais, dependências entre campos).
- **Legibilidade**: Sintaxe fluida é mais fácil de ler e manter que *Data Annotations*.

### Integração com Model Binding
O *FluentValidation* se integra ao pipeline do ASP.NET Core, preenchendo o `ModelState` automaticamente quando configurado, permitindo validação fluida sem alterar o fluxo padrão.

---

## Tutorial Passo a Passo

### Passo 1: Instalar o Pacote

Adicione o pacote *FluentValidation.AspNetCore* via NuGet:
```
dotnet add package FluentValidation.AspNetCore
```

### Passo 2: Criar o Modelo

Crie uma classe simples, sem *Data Annotations*, pois as regras serão definidas no validador.

```csharp
public class UsuarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int Idade { get; set; }
    public string Email { get; set; }
}
```

### Passo 3: Definir o Validador

Crie uma classe validadora herdando de `AbstractValidator<T>`.

```csharp
using FluentValidation;

public class UsuarioValidator : AbstractValidator<UsuarioModel>
{
    public UsuarioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID é obrigatório")
            .GreaterThan(0).WithMessage("O ID deve ser maior que 0");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .Length(2, 50).WithMessage("O nome deve ter entre 2 e 50 caracteres")
            .Must(nome => char.IsUpper(nome[0])).WithMessage("O nome deve começar com maiúscula");

        RuleFor(x => x.Idade)
            .InclusiveBetween(18, 100).WithMessage("A idade deve estar entre 18 e 100");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail inválido")
            .When(x => !string.IsNullOrEmpty(x.Email)); // Só valida se não estiver vazio
    }
}
```

### Passo 4: Configurar no Program.cs

Registre o *FluentValidation* no contêiner de DI.

```csharp
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona controladores e FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<UsuarioValidator>();
        fv.ImplicitlyValidateChildProperties = true;
    });

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

### Passo 5: Configurar o Endpoint

Crie um controlador que usa o `ModelState` para validação.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    [HttpPost]
    public IActionResult Criar([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return Ok(new { Mensagem = "Usuário válido", Dados = usuario });
    }
}
```

### Passo 6: Testar a Validação

- **Teste Válido**: `POST /api/usuarios` com `{ "id": 1, "nome": "João", "idade": 25, "email": "joao@email.com" }`
  - Resposta: `200 OK` com `{ "mensagem": "Usuário válido", "dados": { "id": 1, "nome": "João", "idade": 25, "email": "joao@email.com" } }`
- **Teste Inválido**: `POST /api/usuarios` com `{ "id": 0, "nome": "joão", "idade": 15, "email": "invalido" }`
  - Resposta: `400 Bad Request` com:
    ```json
    {
        "Id": ["O ID deve ser maior que 0"],
        "Nome": ["O nome deve começar com maiúscula"],
        "Idade": ["A idade deve estar entre 18 e 100"],
        "Email": ["E-mail inválido"]
    }
    ```

---

## Boas Práticas

1. **Separe validadores**: Crie uma classe validadora por modelo para organização.
2. **Use condições**: Aproveite `When` e `Unless` para validações condicionais.
3. **Mensagens claras**: Personalize mensagens para orientar o usuário.
4. **Teste cenários**: Valide casos extremos (nulos, valores inválidos, etc.).
5. **Combine com serviços**: Injete dependências nos validadores (ex.: verificar unicidade no banco) usando o construtor.

---

## Conclusão

A validação fluente com *FluentValidation* no ASP.NET Core oferece uma alternativa poderosa às *Data Annotations*, com maior flexibilidade e separação de lógica. Este tutorial mostra como instalar, configurar e usar a biblioteca para validar modelos de forma expressiva, integrando-se ao fluxo padrão do framework. É ideal para regras complexas ou projetos que exigem manutenção simplificada.

