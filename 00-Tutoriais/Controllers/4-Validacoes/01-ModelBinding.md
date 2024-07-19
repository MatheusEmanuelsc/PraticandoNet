# Tutorial Completo sobre Model Binding no ASP.NET Core

## Índice
1. [Introdução](#introdução)
2. [Funcionamento Básico do Model Binding](#funcionamento-básico-do-model-binding)
    - [Fontes de Dados](#fontes-de-dados)
    - [Ciclo de Vida](#ciclo-de-vida)
3. [Exemplos Práticos](#exemplos-práticos)
    - [Binding Simples com Parâmetros de Rota](#binding-simples-com-parâmetros-de-rota)
    - [Binding com Query String](#binding-com-query-string)
    - [Binding com Formulário](#binding-com-formulário)
    - [Binding com JSON no Corpo da Requisição](#binding-com-json-no-corpo-da-requisição)
    - [Binding com Serviços](#binding-com-serviços)
4. [Validações de Modelo](#validações-de-modelo)
    - [Validações Implícitas](#validações-implícitas)
    - [Validações Personalizadas](#validações-personalizadas)
5. [Configurações Avançadas](#configurações-avançadas)
    - [Provedores de Model Binding](#provedores-de-model-binding)
    - [Atributos de Vinculação](#atributos-de-vinculação)
6. [Conclusão](#conclusão)

## Introdução
O **Model Binding** é um recurso fundamental do ASP.NET Core que permite converter dados enviados em uma requisição HTTP (como formulários, parâmetros de query string, cabeçalhos, etc.) em objetos .NET. Isso facilita o trabalho com dados na camada de aplicação, abstraindo a complexidade de manipulação direta dos dados da requisição.

## Funcionamento Básico do Model Binding

### Fontes de Dados
O Model Binding no ASP.NET Core pode utilizar várias fontes de dados:
- **Query Strings**: Dados enviados na URL após o símbolo `?`.
- **Rota**: Dados contidos na URL como parte da rota.
- **Formulário**: Dados enviados através de um formulário HTML.
- **Cabeçalhos de Requisição**: Dados nos cabeçalhos HTTP.
- **Corpo da Requisição**: Dados enviados no corpo da requisição, normalmente em formato JSON.
- **Serviços**: Instâncias de serviços registrados no contêiner de injeção de dependência.

### Ciclo de Vida
1. **Recepção da Requisição**: A aplicação ASP.NET Core recebe uma requisição HTTP.
2. **Seleção do Controlador e Ação**: O roteamento identifica o controlador e a ação que deve processar a requisição.
3. **Model Binding**: O ASP.NET Core vincula os dados da requisição aos parâmetros da ação do controlador.

## Exemplos Práticos

### Binding Simples com Parâmetros de Rota

```csharp
[Route("products/{id}")]
public IActionResult GetProduct(int id)
{
    // O 'id' é vinculado automaticamente a partir da URL.
    // Exemplo de URL: /products/5
}
```

### Binding com Query String

```csharp
public IActionResult Search([FromQuery] string name)
{
    // O 'name' é vinculado automaticamente a partir da query string.
    // Exemplo de URL: /search?name=Laptop
}
```

### Binding com Formulário

```csharp
[HttpPost]
public IActionResult SubmitForm([FromForm] UserModel user)
{
    // O 'user' é vinculado automaticamente a partir dos dados do formulário.
}
```

```csharp
public class UserModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

### Binding com JSON no Corpo da Requisição

```csharp
[HttpPost]
public IActionResult CreateProduct([FromBody] ProductModel product)
{
    // O 'product' é vinculado automaticamente a partir do JSON no corpo da requisição.
}
```

```csharp
public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Binding com Serviços

```csharp
public class MyService
{
    public string GetData() => "Hello from MyService";
}

public class MyController : Controller
{
    [HttpGet]
    public IActionResult GetData([FromServices] MyService myService)
    {
        var data = myService.GetData();
        return Ok(data);
    }
}
```

## Validações de Modelo

### Validações Implícitas
O ASP.NET Core oferece suporte a validações automáticas utilizando anotações de dados (data annotations):

```csharp
public class UserModel
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }
}
```

```csharp
[HttpPost]
public IActionResult SubmitForm([FromForm] UserModel user)
{
    if (ModelState.IsValid)
    {
        // Processar o formulário
    }
    else
    {
        // Tratar erros de validação
    }
}
```

### Validações Personalizadas
Você pode criar validações personalizadas implementando a interface `IValidatableObject`:

```csharp
public class UserModel : IValidatableObject
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FirstName == LastName)
        {
            yield return new ValidationResult("First name and last name cannot be the same.", new[] { "FirstName", "LastName" });
        }
    }
}
```

## Configurações Avançadas

### Provedores de Model Binding
Você pode criar provedores personalizados para vinculação de modelos, estendendo a funcionalidade padrão:

```csharp
public class CustomModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue("custom").FirstValue;
        bindingContext.Result = ModelBindingResult.Success(value);
        return Task.CompletedTask;
    }
}
```

### Atributos de Vinculação
Os atributos como `[FromQuery]`, `[FromRoute]`, `[FromForm]`, `[FromBody]` e `[FromServices]` ajudam a especificar de onde os dados devem ser vinculados.

## Conclusão
O Model Binding no ASP.NET Core simplifica significativamente a manipulação de dados nas aplicações web, permitindo que os desenvolvedores trabalhem com objetos de modelo fortemente tipados em vez de manipular diretamente os dados da requisição HTTP. Com as capacidades de configuração e personalização, o Model Binding pode ser adaptado para atender a requisitos específicos e complexos de aplicação.

Este tutorial abordou desde conceitos básicos até configurações avançadas, fornecendo uma visão abrangente sobre como utilizar e estender o Model Binding no ASP.NET Core.