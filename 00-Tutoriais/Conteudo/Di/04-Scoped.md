

# Scoped e Injeção de Dependência no .NET 8  
Este tutorial aborda o padrão **Scoped**, que cria uma instância única por escopo de solicitação. Vamos explorar sua implementação e como utilizá-lo com injeção de dependência em ASP.NET Core.

---

## Índice  
1. [O que é o Scoped?](#o-que-é-o-scoped)  
2. [Configuração do Projeto](#configuração-do-projeto)  
3. [Implementação do Serviço Scoped](#implementação-do-serviço-scoped)  
4. [Configuração na Classe `Program`](#configuração-na-classe-program)  
5. [Uso do Scoped no Controller](#uso-do-scoped-no-controller)  
6. [Testando o Funcionamento](#testando-o-funcionamento)  

---

## O que é o Scoped?  
O padrão **Scoped** garante que o serviço seja instanciado uma única vez por escopo de solicitação HTTP. Isso significa que dentro da mesma requisição, todas as dependências resolvidas compartilharão a mesma instância.

### Características do Scoped:  
- Cria uma única instância por escopo de solicitação HTTP.  
- Reutiliza a mesma instância dentro de uma requisição, mas cria uma nova para cada nova requisição.  
- Ideal para serviços que precisam de consistência ao longo de uma solicitação HTTP.  

---

## Configuração do Projeto  
1. **Crie um novo projeto Web API em .NET 8**:  
   ```bash
   dotnet new webapi -n ScopedExample
   cd ScopedExample
   ```

2. **Estrutura inicial do projeto**:  
   O projeto inclui a classe `Program.cs` e a pasta `Controllers`.

---

## Implementação do Serviço Scoped  
1. **Crie o serviço `ScopedService`**:  

   No diretório raiz, crie a pasta `Services` e adicione a classe `ScopedService`:  
   ```csharp
   namespace ScopedExample.Services;

   public class ScopedService
   {
       private readonly Guid _serviceId;

       public ScopedService()
       {
           _serviceId = Guid.NewGuid();
       }

       public Guid GetServiceId()
       {
           return _serviceId;
       }
   }
   ```

   ### Explicação:  
   - O `_serviceId` é um identificador exclusivo gerado no momento da criação da instância.  
   - O método `GetServiceId` retorna o identificador para verificar a reutilização do serviço.

---

## Configuração na Classe `Program`  
1. **Registre o serviço Scoped**:  

   Na classe `Program.cs`, configure o serviço:  
   ```csharp
   using ScopedExample.Services;

   var builder = WebApplication.CreateBuilder(args);

   // Adiciona o serviço Scoped
   builder.Services.AddScoped<ScopedService>();

   var app = builder.Build();

   app.MapControllers();

   app.Run();
   ```

   ### Explicação:  
   - O método `AddScoped<T>()` registra o serviço para criar uma nova instância por escopo de solicitação.

---

## Uso do Scoped no Controller  
1. **Crie o `ScopedController`**:  

   Na pasta `Controllers`, adicione a classe `ScopedController`:  
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using ScopedExample.Services;

   [ApiController]
   [Route("api/[controller]")]
   public class ScopedController : ControllerBase
   {
       private readonly ScopedService _scopedService1;
       private readonly ScopedService _scopedService2;

       // Construtor com injeção de dependência
       public ScopedController(ScopedService scopedService1, ScopedService scopedService2)
       {
           _scopedService1 = scopedService1;
           _scopedService2 = scopedService2;
       }

       [HttpGet("compare")]
       public IActionResult CompareInstances()
       {
           var id1 = _scopedService1.GetServiceId();
           var id2 = _scopedService2.GetServiceId();

           return Ok(new
           {
               Instance1 = id1,
               Instance2 = id2,
               AreEqual = id1 == id2
           });
       }
   }
   ```

   ### Explicação:  
   - **Dois serviços injetados**: O construtor injeta duas instâncias do `ScopedService`.  
   - **Endpoint `/api/scoped/compare`**: Retorna os IDs das instâncias injetadas e verifica se são iguais.  

---

## Testando o Funcionamento  
1. **Inicie a aplicação**:  
   Execute o projeto:  
   ```bash
   dotnet run
   ```

2. **Teste o endpoint**:  
   Use **Postman**, **Insomnia** ou o navegador para enviar uma requisição GET para:  
   ```
   https://localhost:5001/api/scoped/compare
   ```

3. **Resultados esperados**:  
   - **Mesma requisição**: As instâncias terão IDs iguais, pois o serviço Scoped é compartilhado no escopo da solicitação.  
     ```json
     {
         "Instance1": "550e8400-e29b-41d4-a716-446655440000",
         "Instance2": "550e8400-e29b-41d4-a716-446655440000",
         "AreEqual": true
     }
     ```
   - **Nova requisição**: Os IDs serão diferentes, pois uma nova instância será criada para cada requisição.

---

## Conclusão  
Neste tutorial, você aprendeu:  
- O conceito e uso do padrão Scoped no ASP.NET Core.  
- Como registrar um serviço Scoped na classe `Program`.  
- Como verificar a reutilização do serviço Scoped dentro do mesmo escopo de solicitação.  

O padrão Scoped é ideal para serviços que precisam de consistência ao longo de uma solicitação HTTP, como manipulação de contexto de usuário ou operações transacionais.