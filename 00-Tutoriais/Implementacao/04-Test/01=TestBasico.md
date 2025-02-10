# Guia Completo para Testes Unitários com xUnit e FluentValidation

## Índice
1. [Introdução](#introducao)
2. [Criando o projeto de testes](#criando-o-projeto-de-testes)
3. [Organização do Projeto de Testes](#organizacao-do-projeto-de-testes)
4. [Estrutura Básica de um Teste](#estrutura-basica-de-um-teste)
5. [Gerando Dados Falsos com Bogus](#gerando-dados-falsos-com-bogus)
6. [Utilizando Fluent Assertions](#utilizando-fluent-assertions)
7. [Exemplos de Testes](#exemplos-de-testes)

---

## 1. Introdução {#introducao}
Testes unitários são fundamentais para garantir a confiabilidade do seu código. Neste guia, vamos criar um conjunto de testes utilizando **xUnit**, **Bogus** para gerar dados falsos e **FluentAssertions** para as asserções.

## 2. Criando o projeto de testes {#criando-o-projeto-de-testes}

Dentro da pasta `tests` do seu projeto, crie um novo projeto xUnit:
```sh
cd tests
dotnet new xunit -n Validators.Tests
```
Agora, adicione a dependência do projeto que será testado:
```sh
dotnet add Validators.Tests reference ../src/Application/Application.csproj
```

## 3. Organização do Projeto de Testes {#organizacao-do-projeto-de-testes}

Estruture o projeto da seguinte maneira:
```
tests/
|-- Validators.Tests/
|   |-- Expenses/
|   |   |-- Register/
|   |   |   |-- RegisterCustomerValidatorTest.cs
```
> **Nota**: O nome da pasta `Expense` foi corrigido para `Costumer`, seguindo a nomenclatura correta.

Cada funcionalidade testada deve ter uma pasta separada, e cada classe de teste deve ter um nome descritivo terminando em `Test`.

## 4. Estrutura Básica de um Teste {#estrutura-basica-de-um-teste}

Os testes unitários são divididos em **três etapas principais**:

1. **Arrange** - Configura o ambiente do teste.
2. **Act** - Executa a funcionalidade testada.
3. **Assert** - Verifica o resultado esperado.

Exemplo:
```csharp
namespace Validators.Tests.Costumers.Register;

public class RegisterCustomerValidatorTest
{
    [Fact]
    public void SuccessTest()
    {
        // Arrange
        var validator = new RegisterCustomerValidator();
        var request = new RequestRegisterCustomerJson
        {
            Name = "João",
            Email = "joao@gmail.com",
            PhoneNumber = "123456"
        };
        
        // Act
        var result = validator.Validate(request);
        
        // Assert
        Assert.True(result.IsValid);
    }
}
```

## 5. Gerando Dados Falsos com Bogus {#gerando-dados-falsos-com-bogus}

Crie um projeto separado para reutilizar geradores de dados falsos:
```sh
dotnet new classlib -n CommonTestUtilities
```
Adicione a dependência ao projeto de testes:
```sh
dotnet add Validators.Tests reference ../CommonTestUtilities/CommonTestUtilities.csproj
```
E instale o pacote `Bogus`:
```sh
dotnet add package Bogus
```

Agora, crie a classe `RequestRegisterCustomerJsonBuilder`:

```csharp
using Bogus;
using CashBank.Communication.Request;

namespace CommonTestUtilities.Request;

public static class RequestRegisterCustomerJsonBuilder
{
    private static readonly Faker<RequestRegisterCustomerJson> faker = new Faker<RequestRegisterCustomerJson>()
        .RuleFor(r => r.Name, f => f.Name.FullName())
        .RuleFor(r => r.Email, f => f.Internet.Email())
        .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber());
    
    public static RequestRegisterCustomerJson Build() => faker.Generate();
}
```

Agora podemos gerar dados falsos automaticamente nos testes:
```csharp
var request = RequestRegisterCustomerJsonBuilder.Build();
```

## 6. Utilizando Fluent Assertions {#utilizando-fluent-assertions}

Instale o pacote FluentAssertions:
```sh
dotnet add Validators.Tests package FluentAssertions
```
Agora podemos melhorar as asserções nos testes:

```csharp
using FluentAssertions;

namespace Validators.Tests.Costumers.Register;

public class RegisterCustomerValidatorTest
{
    [Fact]
    public void SuccessTest()
    {
        // Arrange
        var validator = new RegisterCustomerValidator();
        var request = RequestRegisterCustomerJsonBuilder.Build();
        
        // Act
        var result = validator.Validate(request);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
}
```

## 7. Exemplos de Testes {#exemplos-de-testes}

### Teste para Nome Vazio
```csharp
[Fact]
public void Error_Name_Empty()
{
    // Arrange
    var validator = new RegisterCustomerValidator();
    var request = RequestRegisterCustomerJsonBuilder.Build();
    request.Name = "";
    
    // Act
    var result = validator.Validate(request);
    
    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle().And.Contain(e => e.ErrorMessage.Equals(ResourceErrorMessages.NAME_REQUIRED));
}
```

### Teste para Valores Inválidos
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-2)]
[InlineData(-5)]
public void Error_Amount_Invalid(decimal amount)
{
    // Arrange
    var validator = new RegisterExpenseValidator();
    var request = RequestRegisterExpenseJsonBuilder.Build();
    request.Amount = amount;
    
    // Act
    var result = validator.Validate(request);
    
    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle().And.Contain(e => e.ErrorMessage.Equals(ResourcesErrorMessages.AMOUNT_MUST_BE_GREATER_THAN_0));
}
```

Com isso, temos uma base sólida para testes unitários no .NET usando xUnit, Bogus e FluentAssertions. 🚀

