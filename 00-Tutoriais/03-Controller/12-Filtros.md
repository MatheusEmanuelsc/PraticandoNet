# Filtros em Web API .NET

## Índice
- [Introdução aos Filtros](#introdução-aos-filtros)
- [Tipos de Filtros](#tipos-de-filtros)
- [Implementação Básica](#implementação-básica)
- [Filtros Dinâmicos](#filtros-dinâmicos)
- [Combinando com Paginação](#combinando-com-paginação)
- [Expressões Lambda Dinâmicas](#expressões-lambda-dinâmicas)
- [Filtros Avançados com Specifications](#filtros-avançados-com-specifications)
- [Exemplo Completo](#exemplo-completo)
- [Bibliotecas e Pacotes](#bibliotecas-e-pacotes)

## Introdução aos Filtros

Os filtros são componentes essenciais em Web APIs que permitem aos clientes requisitar subconjuntos específicos de dados baseados em critérios. Implementar filtros eficientes em uma API .NET:

- Reduz a quantidade de dados transferidos
- Melhora o desempenho da API
- Proporciona maior flexibilidade aos consumidores
- Diminui o processamento no lado do cliente

## Tipos de Filtros

Numa Web API .NET, você pode implementar diferentes tipos de filtros:

1. **Filtros Simples**: Baseados em igualdade exata de valores
2. **Filtros de Intervalo**: Para dados numéricos ou datas (min/max)
3. **Filtros de Texto**: Contém, começa com, termina com
4. **Filtros Compostos**: Combinação de múltiplos critérios com operadores lógicos
5. **Filtros Dinâmicos**: Baseados em expressões construídas em runtime

## Implementação Básica

A implementação básica de filtros envolve adicionar parâmetros à ação do controlador:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos(
    [FromQuery] string nome,
    [FromQuery] decimal? precoMinimo,
    [FromQuery] decimal? precoMaximo,
    [FromQuery] int? categoriaId)
{
    var query = _context.Produtos.AsQueryable();
    
    if (!string.IsNullOrEmpty(nome))
        query = query.Where(p => p.Nome.Contains(nome));
        
    if (precoMinimo.HasValue)
        query = query.Where(p => p.Preco >= precoMinimo.Value);
        
    if (precoMaximo.HasValue)
        query = query.Where(p => p.Preco <= precoMaximo.Value);
        
    if (categoriaId.HasValue)
        query = query.Where(p => p.CategoriaId == categoriaId.Value);
    
    return await query.ToListAsync();
}
```

## Filtros Dinâmicos

Para filtros mais flexíveis, podemos criar uma classe de parâmetros de filtro:

```csharp
public class ProdutoFilterParameters
{
    public string Nome { get; set; }
    public decimal? PrecoMinimo { get; set; }
    public decimal? PrecoMaximo { get; set; }
    public int? CategoriaId { get; set; }
    public DateTime? DataCadastroApos { get; set; }
    public bool? Disponivel { get; set; }
    public string TextoDescricao { get; set; }
}

[HttpGet]
public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos(
    [FromQuery] ProdutoFilterParameters filtro)
{
    var query = _context.Produtos.AsQueryable();
    
    query = ApplyFilters(query, filtro);
    
    return await query.ToListAsync();
}

private IQueryable<Produto> ApplyFilters(
    IQueryable<Produto> query, 
    ProdutoFilterParameters filtro)
{
    if (!string.IsNullOrEmpty(filtro.Nome))
        query = query.Where(p => p.Nome.Contains(filtro.Nome));
        
    if (filtro.PrecoMinimo.HasValue)
        query = query.Where(p => p.Preco >= filtro.PrecoMinimo.Value);
        
    if (filtro.PrecoMaximo.HasValue)
        query = query.Where(p => p.Preco <= filtro.PrecoMaximo.Value);
        
    if (filtro.CategoriaId.HasValue)
        query = query.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        
    if (filtro.DataCadastroApos.HasValue)
        query = query.Where(p => p.DataCadastro >= filtro.DataCadastroApos.Value);
        
    if (filtro.Disponivel.HasValue)
        query = query.Where(p => p.Disponivel == filtro.Disponivel.Value);
        
    if (!string.IsNullOrEmpty(filtro.TextoDescricao))
        query = query.Where(p => p.Descricao.Contains(filtro.TextoDescricao));
    
    return query;
}
```

## Combinando com Paginação

A combinação de filtros com paginação é uma prática comum:

```csharp
[HttpGet]
public async Task<ActionResult<PagedResponse<ProdutoDto>>> GetProdutos(
    [FromQuery] ProdutoFilterParameters filtro,
    [FromQuery] PaginationParameters paginacao)
{
    var query = _context.Produtos.AsQueryable();
    
    // Aplicar filtros
    query = ApplyFilters(query, filtro);
    
    // Contar total após filtrar mas antes de paginar
    var totalItems = await query.CountAsync();
    
    // Aplicar ordenação e paginação
    var produtos = await query
        .OrderBy(p => p.Nome) // Ou ordenação dinâmica
        .Skip((paginacao.PageNumber - 1) * paginacao.PageSize)
        .Take(paginacao.PageSize)
        .Select(p => new ProdutoDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Preco = p.Preco
        })
        .ToListAsync();
    
    var response = new PagedResponse<ProdutoDto>
    {
        Data = produtos,
        PageNumber = paginacao.PageNumber,
        PageSize = paginacao.PageSize,
        TotalPages = (int)Math.Ceiling(totalItems / (double)paginacao.PageSize),
        TotalRecords = totalItems
    };
    
    return Ok(response);
}
```

## Expressões Lambda Dinâmicas

Para filtros mais complexos, você pode usar expressões lambda dinâmicas:

```csharp
using System.Linq.Expressions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query, 
        bool condition, 
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}

// Uso
query = query
    .WhereIf(!string.IsNullOrEmpty(filtro.Nome), 
        p => p.Nome.Contains(filtro.Nome))
    .WhereIf(filtro.PrecoMinimo.HasValue, 
        p => p.Preco >= filtro.PrecoMinimo.Value)
    .WhereIf(filtro.PrecoMaximo.HasValue, 
        p => p.Preco <= filtro.PrecoMaximo.Value);
```

## Filtros Avançados com Specifications

Para aplicações mais complexas, o padrão Specification é útil:

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
}

public class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
    public List<string> IncludeStrings { get; } = new List<string>();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    
    protected BaseSpecification() { }
    
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
    
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
    
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
}

// Exemplo de uma specification concreta
public class ProdutoFilterSpecification : BaseSpecification<Produto>
{
    public ProdutoFilterSpecification(ProdutoFilterParameters filtro) 
        : base(BuildCriteria(filtro))
    {
        if (!string.IsNullOrEmpty(filtro.OrderBy))
        {
            switch (filtro.OrderBy.ToLower())
            {
                case "preco":
                    if (filtro.OrderDescending)
                        ApplyOrderByDescending(p => p.Preco);
                    else
                        ApplyOrderBy(p => p.Preco);
                    break;
                case "nome":
                default:
                    if (filtro.OrderDescending)
                        ApplyOrderByDescending(p => p.Nome);
                    else
                        ApplyOrderBy(p => p.Nome);
                    break;
            }
        }
        
        // Incluir relacionamentos se necessário
        AddInclude(p => p.Categoria);
    }
    
    private static Expression<Func<Produto, bool>> BuildCriteria(ProdutoFilterParameters filtro)
    {
        var predicate = PredicateBuilder.True<Produto>();
        
        if (!string.IsNullOrEmpty(filtro.Nome))
            predicate = predicate.And(p => p.Nome.Contains(filtro.Nome));
            
        if (filtro.PrecoMinimo.HasValue)
            predicate = predicate.And(p => p.Preco >= filtro.PrecoMinimo.Value);
            
        if (filtro.PrecoMaximo.HasValue)
            predicate = predicate.And(p => p.Preco <= filtro.PrecoMaximo.Value);
            
        if (filtro.CategoriaId.HasValue)
            predicate = predicate.And(p => p.CategoriaId == filtro.CategoriaId.Value);
            
        return predicate;
    }
}
```

## Exemplo Completo

## Bibliotecas e Pacotes

Existem várias bibliotecas que facilitam a implementação de filtros dinâmicos:

1. **System.Linq.Dynamic.Core**: Permite filtrar usando strings:
   ```csharp
   var query = context.Produtos.Where("Nome.Contains(@0) AND Preco >= @1", filtro.Nome, filtro.PrecoMinimo);
   ```

2. **LinqKit**: Oferece um PredicateBuilder mais sofisticado:
   ```csharp
   var predicate = PredicateBuilder.New<Produto>();
   if (!string.IsNullOrEmpty(filtro.Nome))
       predicate = predicate.And(p => p.Nome.Contains(filtro.Nome));
   ```

3. **Specification Pattern**: Bibliotecas como Ardalis.Specification implementam o padrão Specification:
   ```csharp
   var spec = new ProdutosByFilterSpec(filtro);
   var produtos = await _repository.ListAsync(spec);
   ```

4. **GraphQL**: Para APIs que precisam de filtros altamente dinâmicos:
   ```csharp
   // Usando HotChocolate
   services.AddGraphQLServer()
       .AddQueryType<Query>()
       .AddFiltering()
       .AddSorting();
   ```

5. **OData**: Protocolo para consulta de dados:
   ```csharp
   services.AddControllers().AddOData(opt => 
       opt.AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
          .Select().Filter().OrderBy().Count().Expand());
   ```

## Boas Práticas para Filtros

1. **Validação de Filtros**: Valide os parâmetros de filtro para evitar performance ruim ou erros:
   ```csharp
   if (filtro.PrecoMinimo > filtro.PrecoMaximo)
       return BadRequest("Preço mínimo não pode ser maior que o máximo");
   ```

2. **Documentação com Swagger**: Documente os parâmetros de filtro:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   [Produces("application/json")]
   public class ProdutosController : ControllerBase
   {
       /// <summary>
       /// Obtém produtos com filtros
       /// </summary>
       /// <param name="nome">Filtra por parte do nome</param>
       /// <param name="precoMinimo">Preço mínimo</param>
       /// <response code="200">Retorna produtos filtrados</response>
       [HttpGet]
       [ProducesResponseType(StatusCodes.Status200OK)]
       public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos([FromQuery] string nome, [FromQuery] decimal? precoMinimo)
       {
           // Implementação...
       }
   }
   ```

3. **Cache de Resultados**: Implemente cache para consultas frequentes:
   ```csharp
   [HttpGet]
   [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "nome", "precoMinimo" })]
   public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos([FromQuery] ProdutoFilterParameters filtro)
   {
       // Implementação com cache...
   }
   ```

4. **Limite de Complexidade**: Limite a complexidade de consultas para evitar sobrecarga:
   ```csharp
   if (ContaFiltrosAtivos(filtro) > 5)
       return BadRequest("Muitos filtros aplicados. Limite: 5");
   ```

Ao implementar filtros em uma Web API .NET, é importante equilibrar a flexibilidade para o cliente com a performance e segurança do servidor. Os padrões e exemplos acima fornecem uma base sólida para implementar filtros eficientes e robustos.