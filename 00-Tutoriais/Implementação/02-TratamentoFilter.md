### Tratamento Global de Erros em Web API com .NET 8: Implementação do ApiExceptionFilter

Neste tutorial, vamos criar e configurar um filtro global de exceções para uma Web API utilizando .NET 8. Esse filtro garantirá que todas as exceções não tratadas sejam capturadas e registradas, proporcionando uma resposta consistente ao cliente.

### Índice

1. [Configuração do Projeto](#1-configuração-do-projeto)
2. [Criação da Classe `ApiExceptionFilter`](#2-criação-da-classe-apiexceptionfilter)
3. [Configuração dos Serviços no `Program.cs`](#3-configuração-dos-serviços-no-programcs)
4. [Testando o Filtro de Exceções](#4-testando-o-filtro-de-exceções)
5. [Conclusão](#5-conclusão)

### 1. Configuração do Projeto

Crie um novo projeto de Web API em .NET 8, caso ainda não tenha um:

```bash
dotnet new webapi -n ExcecoesAPI
cd ExcecoesAPI
```

### 2. Criação da Classe `ApiExceptionFilter`

Crie um arquivo chamado `ApiExceptionFilter.cs` na pasta `Filters` (crie a pasta se não existir) e adicione o seguinte código:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ExcecoesAPI.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Ocorreu uma exceção não tratada: Status Code 500");

            context.Result = new ObjectResult("Ocorreu um problema ao tratar a sua solicitação: Status Code 500")
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}
```

### 3. Configuração dos Serviços no `Program.cs`

Abra o arquivo `Program.cs` e configure os serviços para utilizar o filtro `ApiExceptionFilter`. Adicione o filtro globalmente nos controladores e configure as opções do JSON:

```csharp
using ExcecoesAPI.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Adc o Codigo Abaixo no caso
// Adicionando o filtro de exceções personalizado
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configuração do pipeline de solicitações HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 4. Testando o Filtro de Exceções

Para testar o filtro de exceções, crie um controlador que lance uma exceção. Crie um arquivo chamado `TestController.cs` na pasta `Controllers` e adicione o seguinte código:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace ExcecoesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("error")]
        public IActionResult GetError()
        {
            throw new Exception("Esta é uma exceção de teste.");
        }
    }
}
```

Execute o projeto e acesse `https://localhost:5001/api/test/error` no navegador ou utilizando o `curl`:

```bash
curl -X GET https://localhost:5001/api/test/error
```

Você deve ver a mensagem de erro personalizada retornada pelo filtro de exceções.

### 5. Conclusão

Neste tutorial, configuramos um filtro global de exceções para tratar erros em uma Web API com .NET 8. Esse filtro registra as exceções e retorna uma resposta amigável ao cliente, garantindo que exceções não tratadas sejam gerenciadas de forma centralizada e consistente. Isso melhora a manutenção e a escalabilidade da aplicação, além de proporcionar uma melhor experiência ao usuário final.