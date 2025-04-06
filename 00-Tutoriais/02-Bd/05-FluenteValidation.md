

### 📘 FluentValidation no ASP.NET Core 8 - Guia Completo

```md
# FluentValidation no ASP.NET Core 8 - Guia Completo

## 🧭 Índice

1. [Visão Geral](#visão-geral)
2. [Instalação](#instalação)
3. [Abordagem Recomendada (Manual e Desacoplada)](#abordagem-recomendada-manual-e-desacoplada)
4. [Validação Automática (AutoValidation)](#validação-automática-autovalidation)
5. [Validação de Atualizações Parciais com JsonPatchDocument](#validação-de-atualizações-parciais-com-jsonpatchdocument)
6. [Abordagem Legada (para compatibilidade)](#abordagem-legada-para-compatibilidade)
7. [Referências](#referências)

---

## 📌 Visão Geral

O FluentValidation é uma biblioteca popular para validação de dados que promove separação de responsabilidades, reutilização de regras e testes mais fáceis. No ASP.NET Core 8, a integração é ainda mais flexível e moderna.

---

## 📦 Instalação

```bash
dotnet add package FluentValidation
```

Para validação automática (opcional):

```bash
dotnet add package FluentValidation.AspNetCore
```

---

## ✅ Abordagem Recomendada (Manual e Desacoplada)

### 1. Criando um DTO

```csharp
public class CreateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 2. Criando um Validator

```csharp
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress();
    }
}
```

### 3. Registrando no `Program.cs`

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
```

### 4. Usando no Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateUserDto dto, [FromServices] IValidator<CreateUserDto> validator)
{
    var validation = await validator.ValidateAsync(dto);
    if (!validation.IsValid)
        return BadRequest(validation.Errors);

    // continuar com lógica de criação
    return Ok();
}
```

---

## ⚙️ Validação Automática (AutoValidation)

> Mais prática, mas menos explícita. Ideal para projetos simples ou APIs pequenas.

### 1. Instalar o pacote

```bash
dotnet add package FluentValidation.AspNetCore
```

### 2. Ativar no `Program.cs`

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
```

### 3. Controller fica limpo

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    // Validação já foi feita antes de chegar aqui
    return Ok();
}
```

Se houver erro de validação, ele será retornado automaticamente com status `400`.

---

## 🧩 Validação de Atualizações Parciais com JsonPatchDocument

Para permitir PATCH parcial com DTOs, recomendamos:

### 1. Criar um DTO para PATCH

```csharp
public class PatchUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 2. Criar um validador parcial

```csharp
public class PatchUserDtoValidator : AbstractValidator<PatchUserDto>
{
    public PatchUserDtoValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.Email is not null, () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress();
        });
    }
}
```

### 3. Usar com JsonPatchDocument

```csharp
[HttpPatch("{id}")]
public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<PatchUserDto> patchDoc,
    [FromServices] IValidator<PatchUserDto> validator)
{
    if (patchDoc == null)
        return BadRequest();

    var dto = new PatchUserDto();

    patchDoc.ApplyTo(dto, ModelState);
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
        return BadRequest(result.Errors);

    // aplicar mudanças e persistir

    return NoContent();
}
```

---

## 📜 Abordagem Legada (para compatibilidade)

### Startup.cs (.NET 6 ou anterior)

```csharp
services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUserDtoValidator>());
```

Essa abordagem foi substituída por `AddValidatorsFromAssemblyContaining<T>()` e `AddFluentValidationAutoValidation()` no ASP.NET Core 8.

---

## 📚 Referências

- [FluentValidation Docs](https://docs.fluentvalidation.net)
- [GitHub - FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [JsonPatch em ASP.NET Core](https://learn.microsoft.com/aspnet/core/web-api/jsonpatch)

