

# Singleton e Injeção de Dependência no .NET 8  
Este tutorial aborda a implementação do padrão Singleton em .NET 8 e como realizar a injeção de dependência desse tipo de serviço em um projeto ASP.NET Core. O Singleton garante que uma única instância de um serviço seja criada e reutilizada durante toda a aplicação.

---

## Índice  
1. [O que é o Singleton?](#o-que-é-o-singleton)  
2. [Configuração do Projeto](#configuração-do-projeto)  
3. [Implementação do Serviço Singleton](#implementação-do-serviço-singleton)  
4. [Configuração na Classe `Program`](#configuração-na-classe-program)  
5. [Uso do Singleton no Controller](#uso-do-singleton-no-controller)  
6. [Testando o Funcionamento](#testando-o-funcionamento)  

---

## O que é o Singleton?  
O padrão **Singleton** é usado para garantir que uma classe tenha apenas uma única instância em toda a aplicação. Em ASP.NET Core, podemos registrar serviços como Singleton para reutilizar a mesma instância durante todo o ciclo de vida do aplicativo.

### Características do Singleton:  
- Uma única instância é compartilhada em toda a aplicação.  
- Ideal para cenários onde o estado do serviço deve ser mantido.  
- Pode causar problemas se o estado do serviço não for gerenciado corretamente (ex.: problemas com threads concorrentes).  

---

## Configuração do Projeto  
1. **Crie um novo projeto Web API em .NET 8**:  
   ```bash
   dotnet new webapi -n SingletonExample
   cd SingletonExample
   ```

2. **Estrutura inicial do projeto**:  
   A estrutura padrão contém a classe `Program.cs`, que será usada para registrar o serviço Singleton, e a pasta `Controllers` para criar nossos endpoints.

---

## Implementação do Serviço Singleton  
1. **Crie o serviço `SingletonService`**:  

   No diretório raiz do projeto, crie uma pasta chamada `Services` e adicione a classe `SingletonService`:  
   ```csharp
   namespace SingletonExample.Services;

   public class SingletonService
   {
       private int _counter = 0;

       public int IncrementCounter()
       {
           _counter++;
           return _counter;
       }
   }
   ```

   - O método `IncrementCounter` incrementa e retorna o valor do contador.  
   - O estado da variável `_counter` será preservado enquanto a aplicação estiver em execução.

---

## Configuração na Classe `Program`  
1. **Registre o serviço Singleton**:  

   Na classe `Program.cs`, adicione a configuração do Singleton:  
   ```csharp
   using SingletonExample.Services;

   var builder = WebApplication.CreateBuilder(args);

   // Adiciona o serviço Singleton
   builder.Services.AddSingleton<SingletonService>();

   var app = builder.Build();

   app.MapControllers();

   app.Run();
   ```

   - O método `AddSingleton<T>()` registra o serviço como Singleton no contêiner de dependências.  

---

## Uso do Singleton no Controller  
1. **Crie o `SingletonController`**:  

   Na pasta `Controllers`, adicione a classe `SingletonController`:  
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using SingletonExample.Services;

   [ApiController]
   [Route("api/[controller]")]
   public class SingletonController : ControllerBase
   {
       private readonly SingletonService _singletonService;

       // Construtor com injeção de dependência
       public SingletonController(SingletonService singletonService)
       {
           _singletonService = singletonService;
       }

       [HttpGet("increment")]
       public IActionResult Increment()
       {
           var count = _singletonService.IncrementCounter();
           return Ok(new { Count = count });
       }
   }
   ```

   ### Explicação:  
   - **Construtor com injeção de dependência**: O `SingletonService` é injetado diretamente no controlador.  
   - **Endpoint `/api/singleton/increment`**: Incrementa o contador e retorna o valor atual.

---

## Testando o Funcionamento  
1. **Inicie a aplicação**:  
   Execute o projeto:  
   ```bash
   dotnet run
   ```

2. **Teste o endpoint**:  
   Use o **Postman**, **Insomnia** ou o navegador para enviar requisições GET para:  
   ```
   https://localhost:5001/api/singleton/increment
   ```

3. **Resultado esperado**:  
   A cada requisição, o valor do contador será incrementado:  
   ```json
   { "Count": 1 }
   { "Count": 2 }
   { "Count": 3 }
   ```

   O estado do contador é preservado entre as requisições, demonstrando o comportamento do Singleton.

---

## Conclusão  
Neste tutorial, aprendemos:  
- O conceito e o uso do Singleton.  
- Como registrar um serviço como Singleton na classe `Program`.  
- Como usar injeção de dependência para acessar o Singleton no Controller.  
- A importância do gerenciamento de estado em serviços Singleton.  

Você pode expandir esse exemplo para serviços mais complexos, mas lembre-se de avaliar o impacto do estado compartilhado na aplicação.  