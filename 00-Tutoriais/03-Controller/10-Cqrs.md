# Tutorial sobre CQRS no ASP.NET

## Índice
1. [Introdução ao CQRS](#introdução-ao-cqrs)
2. [Estrutura básica do CQRS](#estrutura-básica-do-cqrs)
3. [Comandos (Commands)](#comandos-commands)
4. [Consultas (Queries)](#consultas-queries)
5. [Mediadores com MediatR](#mediadores-com-mediatr)
6. [Configuração no ASP.NET Core](#configuração-no-aspnet-core)
7. [Validações com FluentValidation](#validações-com-fluentvalidation)
8. [Exemplo prático de implementação](#exemplo-prático-de-implementação)
9. [Considerações de desempenho](#considerações-de-desempenho)
10. [Quando usar CQRS](#quando-usar-cqrs)

## Introdução ao CQRS

CQRS (Command Query Responsibility Segregation) é um padrão arquitetural que separa as operações de leitura (Queries) das operações de escrita (Commands) em um sistema. Essa separação permite otimizar cada lado independentemente, melhorando a escalabilidade, manutenção e desempenho da aplicação.

### Benefícios do CQRS:
- Separação clara de responsabilidades
- Escalabilidade independente de leitura e escrita
- Modelos otimizados para cada operação
- Melhor testabilidade
- Mais adequado para sistemas complexos

## Estrutura básica do CQRS

No CQRS, dividimos nossa aplicação em dois fluxos principais:

- **Commands**: Operações que alteram o estado do sistema (criar, atualizar, excluir)
- **Queries**: Operações que apenas leem dados sem alterar o estado

## Comandos (Commands)

Os comandos representam intenções de mudança no sistema e geralmente seguem uma estrutura:

```csharp
public class CreateProductCommand : IRequest<ProductCreatedResult>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

Os manipuladores de comandos implementam a lógica de negócios:

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductCreatedResult>
{
    private readonly IProductRepository _repository;
    
    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ProductCreatedResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            Description = command.Description
        };
        
        await _repository.AddProductAsync(product, cancellationToken);
        
        return new ProductCreatedResult { Id = product.Id };
    }
}
```

## Consultas (Queries)

As consultas recuperam dados sem alterá-los:

```csharp
public class GetProductByIdQuery : IRequest<ProductDto>
{
    public int Id { get; set; }
}
```

Manipuladores de consultas implementam a lógica de recuperação de dados:

```csharp
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductReadRepository _readRepository;
    
    public GetProductByIdQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }
    
    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _readRepository.GetByIdAsync(query.Id, cancellationToken);
        
        if (product == null)
            return null;
            
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description
        };
    }
}
```

## Mediadores com MediatR

MediatR é uma biblioteca popular para implementar o padrão mediator em .NET, que facilita a implementação do CQRS:

```csharp
public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
}

public async Task<IActionResult> GetProduct(int id)
{
    var query = new GetProductByIdQuery { Id = id };
    var product = await _mediator.Send(query);
    
    if (product == null)
        return NotFound();
        
    return Ok(product);
}
```

## Configuração no ASP.NET Core

Como não existe mais a classe Startup no ASP.NET Core moderno, a configuração do CQRS com MediatR é feita no arquivo Program.cs:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Adicionar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar repositórios
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductReadRepository, ProductReadRepository>();

// Adicionar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Adicionar pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// Configuração de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Validações com FluentValidation

Adicione validações aos seus comandos com FluentValidation:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
```

Integre com MediatR usando um pipeline behavior:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

## Exemplo prático de implementação

Estrutura de diretórios recomendada:

```
/Features
  /Products
    /Commands
      CreateProduct.cs  (Command + Handler + Validator)
      UpdateProduct.cs
      DeleteProduct.cs
    /Queries
      GetProductById.cs
      GetAllProducts.cs
  /Orders
    /Commands
    /Queries
/Domain
  /Entities
  /Repositories
/Infrastructure
  /Data
  /Services
/Api
  /Controllers
```

Exemplo de controller usando CQRS:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductCreatedResult>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest();
            
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id });
        return NoContent();
    }
}
```

## Considerações de desempenho

- Use tipos de retorno otimizados para consultas
- Considere a separação de bancos de dados para leitura e escrita em sistemas de alta escala
- Utilize caching para consultas frequentes
- Implemente paginação para grandes conjuntos de dados

## Quando usar CQRS

CQRS é mais adequado para:
- Aplicações complexas de domínio
- Sistemas com diferentes requisitos de escala para leitura e escrita
- Quando há necessidade de modelos de leitura e escrita diferentes
- Para facilitar a testabilidade da lógica de negócios

No entanto, para aplicações simples, CQRS pode adicionar complexidade desnecessária.

---

Este tutorial fornece um guia completo para implementar o padrão CQRS no ASP.NET Core moderno, considerando a ausência da classe Startup e utilizando as melhores práticas atuais.