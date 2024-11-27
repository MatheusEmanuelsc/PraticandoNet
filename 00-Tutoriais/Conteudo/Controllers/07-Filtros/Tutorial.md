 

### Resumo da Implementação

Implementaremos uma classe para definir os critérios de filtragem, uma interface para definir os métodos de filtragem e uma classe que implementa essa interface. Em seguida, adicionaremos um método na controller para realizar a filtragem dos dados.

### 1. Definição da Classe para Critérios de Filtragem

Primeiro, criaremos uma classe para definir os critérios de filtragem, `ProdutosFiltroPreco`:

```csharp
public class ProdutosFiltroPreco : QueryStringParameters
{
    public decimal? Preco { get; set; }
    public string? PrecoCriterio { get; set; } // "maior", "menor" ou "igual"
}
```

### 2. Definição da Interface

Agora, definimos a interface `IProdutoRepository` com os métodos necessários:

```csharp
public interface IProdutoRepository : IRepository<Produto>
{
    PagedList<Produto> GetProdutos(ProdutosParameters produtosParams);
    PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParams);
}
```

### 3. Implementação da Interface

Implementamos a interface na classe `ProdutoRepository`:

```csharp
public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context)
    {}

    public PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParams)
    {
        var produtos = GetAll().AsQueryable();

        if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
        {
            if (produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
        }

        var produtosFiltrados = PagedList<Produto>.ToPagedList(produtos, produtosFiltroParams.PageNumber, produtosFiltroParams.PageSize);
        return produtosFiltrados;
    }
}
```

### 4. Método na Controller

Finalmente, adicionamos um método na controller para filtrar os dados:

```csharp
[HttpGet("filter/preco/pagination")]
public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFilterParameters)
{
    var produtos = _uof.ProdutoRepository.GetProdutosFiltroPreco(produtosFilterParameters);
    return ObterProdutos(produtos);
}
```

### Explicação

1. **Classe ProdutosFiltroPreco:** Define os critérios para filtrar os produtos pelo preço, incluindo o valor do preço e o critério ("maior", "menor", "igual").
2. **Interface IProdutoRepository:** Declara métodos para obter produtos com e sem filtro de preço.
3. **Classe ProdutoRepository:** Implementa a lógica de filtragem de produtos com base nos critérios de preço fornecidos.
4. **Método na Controller:** Exponha um endpoint para permitir que clientes filtrem produtos pelo preço e paginem os resultados.

Essa estrutura fornece uma base sólida para filtrar e paginar produtos com base no preço usando ASP.NET Core e .NET 8.