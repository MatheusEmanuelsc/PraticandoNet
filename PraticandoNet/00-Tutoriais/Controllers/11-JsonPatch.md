# JSON Patch na API Web do ASP.NET Core

**Artigo publicado em: 30/11/2023**  
**Colaboradores: 9**

Este artigo explica como lidar com solicitações JSON Patch em uma API Web do ASP.NET Core.

## Índice

- [Instalação do Pacote](#instalação-do-pacote)
- [Adicionar Suporte ao JSON Patch ao Usar System.Text.Json](#adicionar-suporte-ao-json-patch-ao-usar-systemtextjson)
- [Método de Solicitação HTTP PATCH](#método-de-solicitação-http-patch)
- [JSON Patch](#json-patch)
  - [Sintaxe de Path](#sintaxe-de-path)
  - [Operações](#operações)
    - [Operação add](#a-operação-add)
    - [Operação remove](#a-operação-remove)
    - [Operação replace](#a-operação-replace)
    - [Operação move](#a-operação-move)
    - [Operação copy](#a-operação-copy)
    - [Operação test](#a-operação-test)
- [JSON Patch no ASP.NET Core](#json-patch-no-aspnet-core)
  - [Código do Método de Ação](#código-do-método-de-ação)
  - [Estado do Modelo](#estado-do-modelo)
  - [Objetos Dinâmicos](#objetos-dinâmicos)
- [Obter o Código](#obter-o-código)
- [Recursos Adicionais](#recursos-adicionais)

## Instalação do Pacote

O suporte ao JSON Patch na API Web do ASP.NET Core é baseado em `Newtonsoft.Json` e exige o pacote NuGet `Microsoft.AspNetCore.Mvc.NewtonsoftJson`. Para habilitar o suporte ao JSON Patch:

1. Instale o pacote do NuGet `Microsoft.AspNetCore.Mvc.NewtonsoftJson`.

2. Chame `AddNewtonsoftJson`. Por exemplo:

    ```csharp
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers()
        .AddNewtonsoftJson();

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
    ```

## Adicionar Suporte ao JSON Patch ao Usar System.Text.Json

O formatador de entrada baseado em `System.Text.Json` não suporta JSON Patch. Para adicionar suporte ao JSON Patch usando `Newtonsoft.Json`, deixando inalterados os outros formatadores de entrada e saída:

1. Instale o pacote do NuGet `Microsoft.AspNetCore.Mvc.NewtonsoftJson`.

2. Atualize `Program.cs`:

    ```csharp
    using JsonPatchSample;
    using Microsoft.AspNetCore.Mvc.Formatters;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
    });

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
    ```

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.Options;

    namespace JsonPatchSample;

    public static class MyJPIF
    {
        public static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
    ```

O código anterior cria uma instância de `NewtonsoftJsonPatchInputFormatter` e a insere como a primeira entrada na coleção `MvcOptions.InputFormatters`.

## Método de Solicitação HTTP PATCH

Os métodos PUT e PATCH são usados para atualizar um recurso existente. A diferença entre eles é que PUT substitui o recurso inteiro, enquanto PATCH especifica apenas as alterações.

## JSON Patch

JSON Patch é um formato para especificar as atualizações a serem aplicadas a um recurso. Um documento JSON Patch tem uma matriz de operações. Cada operação identifica um tipo específico de alteração.

### Sintaxe de Path

A propriedade `path` de um objeto de operação tem barras entre os níveis. Por exemplo, `/address/zipCode`.

Índices baseados em zero são usados para especificar os elementos da matriz. O primeiro elemento da matriz `addresses` estaria em `/addresses/0`. Para adicionar ao final de uma matriz, use um hífen (`-`): `/addresses/-`.

### Operações

A tabela a seguir mostra as operações suportadas, conforme definido na especificação JSON Patch:

| Operação | Observações |
|----------|-------------|
| `add`    | Adicione uma propriedade ou elemento de matriz. Para a propriedade existente: defina o valor. |
| `remove` | Remova uma propriedade ou elemento de matriz. |
| `replace`| É o mesmo que remove, seguido por add no mesmo local. |
| `move`   | É o mesmo que remove da origem, seguido por add ao destino usando um valor da origem. |
| `copy`   | É o mesmo que add ao destino usando um valor da origem. |
| `test`   | Retorna o código de status de êxito se o valor em path é igual ao value fornecido. |

#### A Operação add

- Se `path` aponta para um elemento de matriz: insere um novo elemento antes do especificado por `path`.
- Se `path` aponta para uma propriedade: define o valor da propriedade.
- Se `path` aponta para um local não existente:
  - Se o recurso no qual fazer patch é um objeto dinâmico: adiciona uma propriedade.
  - Se o recurso no qual fazer patch é um objeto estático: a solicitação falha.

Exemplo de documento de patch `add`:

```json
[
  {
    "op": "add",
    "path": "/customerName",
    "value": "Barry"
  },
  {
    "op": "add",
    "path": "/orders/-",
    "value": {
      "orderName": "Order2",
      "orderType": null
    }
  }
]
```

#### A Operação remove

- Se `path` aponta para um elemento de matriz: remove o elemento.
- Se `path` aponta para uma propriedade:
  - Se o recurso no qual fazer patch é um objeto dinâmico: remove a propriedade.
  - Se o recurso no qual fazer patch é um objeto estático:
    - Se a propriedade é anulável: define como nulo.
    - Se a propriedade não é anulável: define como `default<T>`.

Exemplo de documento de patch `remove`:

```json
[
  {
    "op": "remove",
    "path": "/customerName"
  },
  {
    "op": "remove",
    "path": "/orders/0"
  }
]
```

#### A Operação replace

Esta operação é funcionalmente a mesma que `remove` seguida por `add`.

Exemplo de documento de patch `replace`:

```json
[
  {
    "op": "replace",
    "path": "/customerName",
    "value": "Barry"
  },
  {
    "op": "replace",
    "path": "/orders/0",
    "value": {
      "orderName": "Order2",
      "orderType": null
    }
  }
]
```

#### A Operação move

- Se `path` aponta para um elemento de matriz: copia o elemento `from` para o local do elemento `path` e, em seguida, executa uma operação `remove` no elemento `from`.
- Se `path` aponta para uma propriedade: copia o valor da propriedade `from` para a propriedade `path`, depois executa uma operação `remove` na propriedade `from`.
- Se `path` aponta para uma propriedade não existente:
  - Se o recurso no qual fazer patch é um objeto estático: a solicitação falha.
  - Se o recurso no qual fazer patch é um objeto dinâmico: copia a propriedade `from` para o local indicado por `path` e, em seguida, executa uma operação `remove` na propriedade `from`.

Exemplo de documento de patch `move`:

```json
[
  {
    "op": "move",
    "from": "/orders/0/orderName",
    "path": "/customerName"
  },
  {
    "op": "move",
    "from": "/orders/1",
    "path": "/orders/0"
  }
]
```

#### A Operação copy

Esta operação é funcionalmente a mesma que uma operação `move`, sem a etapa final `remove`.

Exemplo de documento de patch `copy`:

```json
[
  {
    "op": "copy",
    "from": "/orders/0/orderName",
    "path": "/customerName"
  },
  {
    "op": "copy",
    "from": "/orders/1",
    "path": "/orders/0"
  }
]
```

#### A Operação test

Se o valor no local indicado por `path` for diferente do valor fornecido em `value`, a solicitação falhará. Nesse caso, toda a solicitação de PATCH falhará, mesmo se todas as outras operações no documento de patch forem bem-sucedidas.

Exemplo de documento de patch `test`:

```json
[
 

 {
    "op": "test",
    "path": "/customerName",
    "value": "Nancy"
  },
  {
    "op": "add",
    "path": "/customerName",
    "value": "Barry"
  }
]
```

## JSON Patch no ASP.NET Core

A implementação do ASP.NET Core do JSON Patch é fornecida no pacote NuGet `Microsoft.AspNetCore.JsonPatch`.

### Código do Método de Ação

Em um controlador de API, um método de ação para JSON Patch:

- É anotado com o atributo `HttpPatch`.
- Aceita um `JsonPatchDocument<TModel>`, normalmente com `[FromBody]`.
- Chama `ApplyTo(Object)` no documento de patch para aplicar as alterações.

Exemplo:

```csharp
[HttpPatch]
public IActionResult JsonPatchWithModelState(
    [FromBody] JsonPatchDocument<Customer> patchDoc)
{
    if (patchDoc != null)
    {
        var customer = CreateCustomer();

        patchDoc.ApplyTo(customer, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return new ObjectResult(customer);
    }
    else
    {
        return BadRequest(ModelState);
    }
}
```

Modelo `Customer`:

```csharp
namespace JsonPatchSample.Models;

public class Customer
{
    public string? CustomerName { get; set; }
    public List<Order>? Orders { get; set; }
}
```

Modelo `Order`:

```csharp
namespace JsonPatchSample.Models;

public class Order
{
    public string OrderName { get; set; }
    public string OrderType { get; set; }
}
```

### Estado do Modelo

O exemplo de método de ação anterior chama uma sobrecarga de `ApplyTo` que utiliza o estado do modelo como um de seus parâmetros, permitindo receber mensagens de erro nas respostas. Exemplo de resposta 400 para uma operação `test`:

```json
{
  "Customer": [
    "The current value 'John' at path 'customerName' != test value 'Nancy'."
  ]
}
```

### Objetos Dinâmicos

Exemplo de aplicação de um patch a um objeto dinâmico:

```csharp
[HttpPatch]
public IActionResult JsonPatchForDynamic([FromBody]JsonPatchDocument patch)
{
    dynamic obj = new ExpandoObject();
    patch.ApplyTo(obj);

    return Ok(obj);
}
```

## Obter o Código

Exibir ou baixar o código de exemplo. (Como baixar.)

Para testar o exemplo, execute o aplicativo e envie solicitações HTTP com as seguintes configurações:

- URL: `http://localhost:{port}/jsonpatch/jsonpatchwithmodelstate`
- Método HTTP: PATCH
- Cabeçalho: `Content-Type: application/json-patch+json`
- Corpo: copie e cole um dos exemplos de documento JSON Patch da pasta do projeto JSON.

## Recursos Adicionais

- [Especificação do método PATCH IETF RFC 5789](https://tools.ietf.org/html/rfc5789)
- [Especificação do JSON Patch IETF RFC 6902](https://tools.ietf.org/html/rfc6902)
- [JSON Pointer IETF RFC 6901](https://tools.ietf.org/html/rfc6901)
- [Documentação do JSON Patch](https://jsonpatch.com). Inclui links para recursos para criar documentos JSON Patch.
- [Código-fonte do JSON Patch ASP.NET Core](https://github.com/dotnet/aspnetcore)