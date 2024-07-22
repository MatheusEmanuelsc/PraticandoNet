# Paginação no .NET 8

Este guia detalhado irá mostrar como implementar paginação no .NET 8 utilizando uma base de dados. Vamos percorrer todas as etapas, desde a criação de uma classe genérica para lidar com a paginação até a integração no repositório e no controlador.

## Índice

1. [Introdução](#introdução)
2. [Criando a Classe Genérica de Paginação](#criando-a-classe-genérica-de-paginação)
3. [Criando a Classe de Parâmetros de Paginação](#criando-a-classe-de-parâmetros-de-paginação)
4. [Atualizando o Repositório](#atualizando-o-repositório)
   - [Interface do Repositório](#interface-do-repositório)
   - [Implementação do Repositório](#implementação-do-repositório)
5. [Ajustando o Controlador](#ajustando-o-controlador)
6. [Exemplo Detalhado](#exemplo-detalhado)

## Introdução

A paginação é uma técnica essencial para lidar com grandes conjuntos de dados, permitindo a exibição de partes dos dados de forma paginada. No .NET 8, a classe `Startup` não é mais utilizada, então as implementações podem variar um pouco em comparação com versões anteriores.

## Criando a Classe Genérica de Paginação

Vamos iniciar criando uma classe genérica `PagedList<T>`, que irá gerenciar a lógica da paginação. Esta classe irá encapsular a lógica de paginação e fornecer propriedades úteis como o número total de páginas, se há uma página anterior ou próxima, etc.

```csharp
public class PagedList<T> : List<T> where T : class
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
```

### Explicação:

1. **Propriedades**:
    - `CurrentPage`: A página atual.
    - `TotalPages`: O número total de páginas.
    - `PageSize`: O tamanho da página (número de itens por página).
    - `TotalCount`: O número total de itens.
    - `HasPrevious` e `HasNext`: Indicadores de navegação.

2. **Construtor**:
    - Inicializa as propriedades e calcula o número total de páginas.

3. **Método estático `ToPagedList`**:
    - Recebe uma fonte de dados do tipo `IQueryable<T>`, o número da página e o tamanho da página.
    - Calcula o total de itens e seleciona os itens para a página atual.

## Criando a Classe de Parâmetros de Paginação

Para controlar os parâmetros de paginação, vamos criar a classe `PaginationParameters` e, em seguida, derivar `ProdutosParameters` dela. Esta abordagem permite reutilização e extensão fácil de funcionalidades.

### Classe de Parâmetros de Paginação Genérica

```csharp
namespace APICatalogo.Pagination
{
    public abstract class PaginationParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
```

### Classe de Parâmetros Específicos para Produtos

```csharp
public class ProdutosParameters : PaginationParameters
{
    // Adicione qualquer outro parâmetro específico para a paginação de produtos, se necessário
}
```

### Explicação:

1. **Classe `PaginationParameters`**:
    - `maxPageSize`: Define o tamanho máximo da página.
    - `PageNumber`: Define o número da página atual (valor padrão é 1).
    - `_pageSize`: Campo privado para armazenar o tamanho da página (valor padrão é 10).
    - `PageSize`: Propriedade pública que aplica a lógica para limitar o tamanho da página.

2. **Classe `ProdutosParameters`**:
    - Herda de `PaginationParameters` e pode ser estendida com parâmetros específicos para a paginação de produtos.

## Atualizando o Repositório

### Interface do Repositório

Atualize a interface do repositório para usar `PagedList<Produto>`:

```csharp
public interface IProdutoRepository : IRepository<Produto>
{
    PagedList<Produto> GetProdutos(ProdutosParameters produtosParams);
    IEnumerable<Produto> GetProdutosPorCategoria(int id);
}
```

### Implementação do Repositório

Agora, atualize a implementação do repositório para retornar uma `PagedList<Produto>`:

```csharp
public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context): base(context)
    {       
    }

    public PagedList<Produto> GetProdutos(ProdutosParameters produtosParameters)
    {
        var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable();

        var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, 
                   produtosParameters.PageNumber, produtosParameters.PageSize);
        
        return produtosOrdenados;
    }
}
```

### Explicação:

1. **GetProdutos**:
    - Obtém todos os produtos ordenados por `ProdutoId` como `IQueryable`.
    - Utiliza o método `ToPagedList` para criar uma `PagedList<Produto>` a partir da fonte de dados e parâmetros de paginação.

## Ajustando o Controlador

Por fim, vamos ajustar o controlador para utilizar a paginação e retornar os metadados relevantes.

```csharp
[HttpGet("pagination")]
public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters produtosParameters)
{
    var produtos = _uof.ProdutoRepository.GetProdutos(produtosParameters);

    var metadata = new
    {
        produtos.TotalCount,
        produtos.PageSize,
        produtos.CurrentPage,
        produtos.TotalPages,
        produtos.HasNext,
        produtos.HasPrevious
    };

    Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

    var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
    return Ok(produtosDto);
}
```

### Explicação:

1. **Endpoint `Get`**:
    - Recebe os parâmetros de paginação a partir da query string.
    - Obtém os produtos paginados do repositório.
    - Cria um objeto de metadados com informações sobre a paginação.
    - Adiciona os metadados aos cabeçalhos da resposta.
    - Mapeia a lista de produtos para `ProdutoDTO` e retorna.

## Exemplo Detalhado

Vamos revisar um exemplo completo, explicando cada detalhe:

### Passo 1: Criar a Classe Genérica de Paginação

Esta classe gerencia a lógica de paginação e fornece propriedades úteis:

```csharp
public class PagedList<T> : List<T> where T : class
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
```

### Passo 2: Criar a Classe de Parâmetros de Paginação

Definindo os parâmetros de paginação:

```csharp
namespace APICatalogo.Pagination
{
    public abstract class PaginationParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
           

 }
        }
    }
}

public class ProdutosParameters : PaginationParameters
{
    // Adicione qualquer outro parâmetro específico para a paginação de produtos, se necessário
}
```

### Passo 3: Atualizar a Interface do Repositório

Definindo a interface do repositório para incluir o método de paginação:

```csharp
public interface IProdutoRepository : IRepository<Produto>
{
    PagedList<Produto> GetProdutos(ProdutosParameters produtosParams);
    IEnumerable<Produto> GetProdutosPorCategoria(int id);
}
```

### Passo 4: Implementação do Repositório

Implementando o método de paginação no repositório:

```csharp
public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context): base(context)
    {       
    }

    public PagedList<Produto> GetProdutos(ProdutosParameters produtosParameters)
    {
        var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable();

        var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, 
                   produtosParameters.PageNumber, produtosParameters.PageSize);
        
        return produtosOrdenados;
    }
}
```

### Passo 5: Ajustar o Controlador

Ajustando o controlador para utilizar a paginação e retornar os metadados:

```csharp
[HttpGet("pagination")]
public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters produtosParameters)
{
    var produtos = _uof.ProdutoRepository.GetProdutos(produtosParameters);

    var metadata = new
    {
        produtos.TotalCount,
        produtos.PageSize,
        produtos.CurrentPage,
        produtos.TotalPages,
        produtos.HasNext,
        produtos.HasPrevious
    };

    Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

    var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
    return Ok(produtosDto);
}
```

### Conclusão

Com esses passos, você terá implementado a paginação em uma aplicação .NET 8 utilizando uma abordagem genérica e extensível. Cada componente, desde a classe genérica de paginação até o controlador, foi detalhado para facilitar a compreensão e implementação.