# Métodos de Extensão no .NET

## Índice
1. [Entendendo `WebApplicationBuilder` e `WebApplication`](#entendendo-webapplicationbuilder-e-webapplication)
2. [O que são Métodos de Extensão?](#o-que-sao-metodos-de-extensao)
3. [Métodos de Extensão para WebApplicationBuilder](#metodos-de-extensao-para-webapplicationbuilder)
4. [Métodos de Extensão para WebApplication](#metodos-de-extensao-para-webapplication)
5. [Conclusão](#conclusao)

---

## Entendendo `WebApplicationBuilder` e `WebApplication`

No .NET, ao criar uma aplicação web, utilizamos `WebApplicationBuilder` e `WebApplication`. Cada um possui uma função específica:

- **`WebApplicationBuilder`**: Responsável por configurar e registrar serviços na aplicação antes que ela seja construída. Ele adiciona funcionalidades essenciais, como serviços de logging, autenticação e injeção de dependência.
- **`WebApplication`**: É a aplicação já construída, onde podemos definir middlewares e iniciar o processamento de requisições.

### Exemplo explicativo:

```csharp
var builder = WebApplication.CreateBuilder(args); // Inicializa o builder

builder.Services.AddControllers(); // Adiciona serviços

var app = builder.Build(); // Constrói a aplicação final

app.UseHttpsRedirection(); // Configura middlewares
app.MapControllers();

app.Run(); // Inicia a aplicação e escuta requisições
```

- `CreateBuilder(args)`: Cria um novo `WebApplicationBuilder`.
- `Build()`: Constrói a instância `WebApplication`.
- `Run()`: Executa a aplicação, iniciando o servidor.

Agora que entendemos a estrutura básica, vamos aos métodos de extensão.

---

## O que são Métodos de Extensão?

Os **métodos de extensão** em .NET permitem adicionar funcionalidades a tipos existentes sem modificar diretamente sua implementação. Eles são declarados como métodos `static` dentro de uma classe `static` e utilizam a palavra-chave `this` antes do primeiro parâmetro.

Exemplo básico:
```csharp
public static class StringExtensions
{
    public static bool IsNumeric(this string value)
    {
        return int.TryParse(value, out _);
    }
}
```
Uso:
```csharp
string texto = "123";
bool resultado = texto.IsNumeric(); // Retorna true
```

Agora, aplicamos essa abordagem ao `WebApplicationBuilder` e `WebApplication`.

---

## Métodos de Extensão para `WebApplicationBuilder`

Podemos criar métodos de extensão para organizar a configuração de serviços dentro do `WebApplicationBuilder`.

### Exemplo: Adicionando serviços ao container
```csharp
public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<CashBankContextDb>(config => config.UseMySql(connectionString, serverVersion));
    }
}
```
Uso no `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
```
Isso ajuda a manter o código organizado e modularizado.

---

## Métodos de Extensão para `WebApplication`

Podemos criar métodos de extensão para organizar a configuração de middlewares dentro do `WebApplication`.

### Exemplo: Configurando Middlewares
```csharp
public static class MiddlewareExtensions
{
    public static void UseCustomMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}
```
Uso no `Program.cs`:
```csharp
var app = builder.Build();
app.UseCustomMiddleware();
app.Run();
```
Isso mantém a configuração organizada e reutilizável.

---

## Conclusão

Os **métodos de extensão** são uma excelente maneira de organizar e reutilizar código no .NET. Eles podem ser utilizados para **qualquer tipo**, incluindo `WebApplicationBuilder` e `WebApplication`, tornando a configuração mais modular e limpa. O uso dessa abordagem melhora a legibilidade do código e facilita a manutenção da aplicação.

