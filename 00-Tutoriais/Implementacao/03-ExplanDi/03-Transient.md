

# Transient e Injeção de Dependência no .NET 8  
Este tutorial explica o padrão **Transient**, como configurá-lo em um projeto ASP.NET Core e como utilizá-lo com injeção de dependência. Os serviços registrados como Transient criam uma nova instância toda vez que são solicitados.

---

## Índice  
1. [O que é o Transient?](#o-que-é-o-transient)  
2. [Configuração do Projeto](#configuração-do-projeto)  
3. [Implementação do Serviço Transient](#implementação-do-serviço-transient)  
4. [Configuração na Classe `Program`](#configuração-na-classe-program)  
5. [Uso do Transient no Controller](#uso-do-transient-no-controller)  
6. [Testando o Funcionamento](#testando-o-funcionamento)  

---

## O que é o Transient?  
O padrão **Transient** registra serviços que criam uma nova instância toda vez que são solicitados. Este tipo de serviço é ideal para cenários onde não há necessidade de manter o estado entre solicitações.  

### Características do Transient:  
- Uma nova instância do serviço é criada sempre que ele é solicitado.  
- Recomendado para serviços leves ou sem estado.  
- Pode consumir mais memória e processamento se instanciado frequentemente.  

---

## Configuração do Projeto  
1. **Crie um novo projeto Web API em .NET 8**:  
   ```bash
   dotnet new webapi -n TransientExample
   cd TransientExample
   ```

2. **Estrutura inicial do projeto**:  
   O projeto inclui a classe `Program.cs` para registrar o serviço e a pasta `Controllers` para os endpoints.

---

## Implementação do Serviço Transient  
1. **Crie o serviço `TransientService`**:  

   No diretório raiz do projeto, crie uma pasta chamada `Services` e adicione a classe `TransientService`:  
   ```csharp
   namespace TransientExample.Services;

   public class TransientService
   {
       private readonly Guid _serviceId;

       public TransientService()
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
   - A propriedade `_serviceId` armazena um identificador exclusivo gerado no momento da criação da instância.  
   - O método `GetServiceId` retorna o identificador, permitindo verificar se a instância foi recriada.

---

## Configuração na Classe `Program`  
1. **Registre o serviço Transient**:  

   Na classe `Program.cs`, adicione a configuração do Transient:  
   ```csharp
   using TransientExample.Services;

   var builder = WebApplication.CreateBuilder(args);

   // Adiciona o serviço Transient
   builder.Services.AddTransient<TransientService>();

   var app = builder.Build();

   app.MapControllers();

   app.Run();
   ```

   ### Explicação:  
   - O método `AddTransient<T>()` registra o serviço para criar uma nova instância sempre que ele for solicitado.

---

## Uso do Transient no Controller  
1. **Crie o `TransientController`**:  

   Na pasta `Controllers`, adicione a classe `TransientController`:  
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using TransientExample.Services;

   [ApiController]
   [Route("api/[controller]")]
   public class TransientController : ControllerBase
   {
       private readonly TransientService _transientService1;
       private readonly TransientService _transientService2;

       // Construtor com injeção de dependência
       public TransientController(TransientService transientService1, TransientService transientService2)
       {
           _transientService1 = transientService1;
           _transientService2 = transientService2;
       }

       [HttpGet("compare")]
       public IActionResult CompareInstances()
       {
           var id1 = _transientService1.GetServiceId();
           var id2 = _transientService2.GetServiceId();

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
   - **Dois serviços injetados**: O construtor injeta duas instâncias do `TransientService`.  
   - **Endpoint `/api/transient/compare`**: Retorna os IDs das instâncias injetadas e verifica se elas são diferentes.  

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
   https://localhost:5001/api/transient/compare
   ```

3. **Resultado esperado**:  
   A resposta JSON mostrará dois IDs diferentes, indicando que o serviço foi instanciado duas vezes:  
   ```json
   {
       "Instance1": "550e8400-e29b-41d4-a716-446655440000",
       "Instance2": "660e8400-e29b-41d4-a716-556655440111",
       "AreEqual": false
   }
   ```

---

## Conclusão  
Neste tutorial, aprendemos:  
- O conceito e o uso do Transient em ASP.NET Core.  
- Como registrar um serviço Transient na classe `Program`.  
- Como usar injeção de dependência para acessar o Transient no Controller.  
- Como verificar a criação de novas instâncias para serviços Transient.  

O Transient é útil para serviços leves ou sem estado que precisam ser criados frequentemente, mas deve ser usado com cuidado para evitar desperdício de recursos.  