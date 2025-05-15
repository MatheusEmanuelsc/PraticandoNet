# Paginação em Web API .NET

## Índice
- [Introdução](#introdução)
- [Implementação Básica](#implementação-básica)
- [Modelos de Resposta com Paginação](#modelos-de-resposta-com-paginação)
- [Parâmetros de Paginação](#parâmetros-de-paginação)
- [HATEOAS e Navegação](#hateoas-e-navegação)
- [Otimização de Performance](#otimização-de-performance)
- [Exemplo Completo](#exemplo-completo)
- [Bibliotecas e Pacotes](#bibliotecas-e-pacotes)

## Introdução

A paginação é uma técnica essencial para APIs que retornam conjuntos grandes de dados. Em vez de retornar todos os registros de uma vez, a API divide os resultados em "páginas" menores. Isso traz vários benefícios:

- Reduz o consumo de largura de banda
- Melhora o tempo de resposta da API
- Diminui a carga no servidor
- Facilita o consumo dos dados pelo cliente

No contexto de Web APIs .NET, existem várias abordagens para implementar a paginação de forma eficiente.

## Implementação Básica

A implementação básica envolve:

- Receber parâmetros de página e tamanho
- Aplicar Skip/Take (ou equivalente) na consulta
- Retornar os registros da página solicitada junto com metadados

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
{
    var totalItems = await _context.Produtos.CountAsync();
    
    var produtos = await _context.Produtos
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    // Configurar cabeçalhos com informações da paginação
    Response.Headers.Add("X-Total-Count", totalItems.ToString());
    Response.Headers.Add("X-Page-Number", pageNumber.ToString());
    Response.Headers.Add("X-Page-Size", pageSize.ToString());
    
    return Ok(produtos);
}
```

## Modelos de Resposta com Paginação

É recomendável criar uma classe específica para encapsular dados paginados:

```csharp
public class PagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public long TotalRecords { get; set; }
    public IEnumerable<T> Data { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

Exemplo de uso:

```csharp
[HttpGet]
public async Task<ActionResult<PagedResponse<ProdutoDto>>> GetProdutos(
    [FromQuery] PaginationParameters parameters)
{
    var totalItems = await _context.Produtos.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)parameters.PageSize);
    
    var produtos = await _context.Produtos
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .Select(p => new ProdutoDto { /* mapeamento */ })
        .ToListAsync();
    
    var pagedResponse = new PagedResponse<ProdutoDto>
    {
        Data = produtos,
        PageNumber = parameters.PageNumber,
        PageSize = parameters.PageSize,
        TotalPages = totalPages,
        TotalRecords = totalItems
    };
    
    return Ok(pagedResponse);
}
```

## Parâmetros de Paginação

É comum criar uma classe para encapsular parâmetros de paginação:

```csharp
public class PaginationParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    
    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
```

## HATEOAS e Navegação

Para APIs RESTful mais robustas, você pode implementar HATEOAS (Hypermedia as the Engine of Application State):

```csharp
public class PagedResponseWithLinks<T>
{
    public PagedResponse<T> Pagination { get; set; }
    public IDictionary<string, string> Links { get; set; } = new Dictionary<string, string>();
}

// No controlador
[HttpGet]
public async Task<ActionResult<PagedResponseWithLinks<ProdutoDto>>> GetProdutos(
    [FromQuery] PaginationParameters parameters)
{
    // Lógica de obtenção de dados...
    
    var response = new PagedResponseWithLinks<ProdutoDto>
    {
        Pagination = pagedResponse
    };
    
    // Adicionar links de navegação
    if (pagedResponse.HasPrevious)
        response.Links.Add("previousPage", 
            Url.Link("GetProdutos", new { pageNumber = parameters.PageNumber - 1, pageSize = parameters.PageSize }));
    
    if (pagedResponse.HasNext)
        response.Links.Add("nextPage", 
            Url.Link("GetProdutos", new { pageNumber = parameters.PageNumber + 1, pageSize = parameters.PageSize }));
    
    response.Links.Add("currentPage", 
        Url.Link("GetProdutos", new { pageNumber = parameters.PageNumber, pageSize = parameters.PageSize }));
    
    return Ok(response);
}
```

## Otimização de Performance

Para melhorar a performance em grandes datasets:

- Use `IQueryable<T>` para adiar a execução da consulta
- Considere consultas assíncronas com `ToListAsync()`
- Implemente filtros antes da paginação
- Use projeção para selecionar apenas campos necessários
- Considere usar caching para páginas frequentemente acessadas

```csharp
public async Task<PagedResponse<T>> CreatePagedResultAsync<T>(
    IQueryable<T> source, 
    int pageNumber, 
    int pageSize)
{
    var count = await source.CountAsync();
    var items = await source.Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
    
    return new PagedResponse<T>
    {
        Data = items,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(count / (double)pageSize),
        TotalRecords = count
    };
}
```

## Exemplo Completo

## Bibliotecas e Pacotes

Existem bibliotecas que facilitam a implementação de paginação:

1. **X.PagedList**: Fornece extensões para IQueryable e IEnumerable:
   ```csharp
   var pagedList = await context.Produtos.ToPagedListAsync(pageNumber, pageSize);
   ```

2. **Dynamic LINQ**: Permite ordenação dinâmica usando expressões em string:
   ```csharp
   var produtos = context.Produtos.OrderBy(parameters.OrderBy).Skip(skip).Take(take);
   ```

3. **AutoMapper.Extensions.Microsoft.DependencyInjection**: Útil para mapear entidades em DTOs durante o processo de paginação.

4. **System.Linq.Dynamic.Core**: Facilita consultas dinâmicas:
   ```csharp
   var produtos = context.Produtos.OrderBy(parameters.OrderBy).Skip(skip).Take(take);
   ```

5. **Microsoft.EntityFrameworkCore**: Para otimização das consultas com Include, AsNoTracking e outras opções.

A escolha da biblioteca depende dos requisitos específicos do seu projeto, como a complexidade das consultas, necessidade de ordenação dinâmica e volume de dados.

O exemplo completo acima demonstra uma implementação robusta que você pode adaptar às necessidades do seu projeto.