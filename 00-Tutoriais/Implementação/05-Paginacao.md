# Implementação de Paginação em .NET 8

Este guia passo a passo descreve como implementar a paginação em uma aplicação ASP.NET Core, utilizando uma abordagem genérica para que possa ser reutilizada em diferentes contextos. 

## Índice

1. [Criação de Classe Genérica Abstrata](#etapa-1)
2. [Herdando a Classe Genérica](#etapa-2)
3. [Implementação da Classe PagedList](#etapa-3)
4. [Ajustes no Repositório](#etapa-4)
5. [Implementação no Repositório](#etapa-5)
6. [Método no Controller](#etapa-6)
7. [Resumo](#resumo)

---

## Etapa 1: Criação de Classe Genérica Abstrata

Nesta etapa, vamos criar uma classe abstrata `PaginationParameters` que servirá como base para os parâmetros de paginação.

```csharp
namespace Curso.Api.Pagination
{
    public abstract class PaginationParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = maxPageSize;

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

### Comentários

- **maxPageSize**: Define o tamanho máximo da página.
- **PageNumber**: Número da página atual, iniciando com 1.
- **PageSize**: Tamanho da página, limitado ao valor máximo definido por `maxPageSize`.

---

## Etapa 2: Herdando a Classe Genérica

Agora, vamos criar uma classe que herda `PaginationParameters` para especificar parâmetros de paginação para produtos.

```csharp
public class ProdutosParameters : PaginationParameters
{
    // Adicione qualquer outro parâmetro específico para a paginação de produtos, se necessário
}
```

### Comentários

- **ProdutosParameters**: Herda de `PaginationParameters` e pode conter parâmetros adicionais específicos para produtos.

---

## Etapa 3: Implementação da Classe PagedList

Vamos criar a classe `PagedList<T>`, que encapsula a lógica de paginação e os resultados paginados.

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

    public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
```

### Comentários

- **CurrentPage**: Página atual.
- **TotalPages**: Número total de páginas.
- **PageSize**: Tamanho da página.
- **TotalCount**: Total de itens.
- **HasPrevious**: Indica se há uma página anterior.
- **HasNext**: Indica se há uma próxima página.
- **ToPagedListAsync**: Método assíncrono para criar uma instância de `PagedList<T>` a partir de uma fonte `IQueryable<T>`.

---

## Etapa 4: Ajustes no Repositório

Ajuste a interface do repositório para incluir o método de paginação.

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    T Create(T entity);
    T Update(T entity);
    T Delete(T entity);

    // Método de paginação
    Task<PagedList<T>> GetPagedAsync(PaginationParameters paginationParameters);
}
```

### Comentários

- **GetPagedAsync**: Método para obter dados paginados com base nos parâmetros de paginação.

---

## Etapa 5: Implementação no Repositório

Implemente o método `GetPagedAsync` no repositório.

```csharp
public async Task<PagedList<T>> GetPagedAsync(PaginationParameters paginationParameters)
{
    var source = _context.Set<T>().AsQueryable();
    var count = await source.CountAsync();
    var items = await source.Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                            .Take(paginationParameters.PageSize)
                            .ToListAsync();

    return new PagedList<T>(items, count, paginationParameters.PageNumber, paginationParameters.PageSize);
}
```

### Comentários

- **source**: Consulta a fonte de dados.
- **count**: Conta total de itens na fonte.
- **items**: Itens paginados obtidos com base nos parâmetros de paginação.

---

## Etapa 6: Método no Controller

Implemente o método no controller para utilizar a paginação.

```csharp
[HttpGet]
public async Task<IActionResult> GetItems([FromQuery] AlunoParameters paginationParameters)
{
    var pagedItems = await _unitOfWork.AlunoRepository.GetPagedAsync(paginationParameters);
    var listaAlunosDto = _mapper.Map<IEnumerable<AlunoDto>>(pagedItems);
    return Ok(listaAlunosDto);
}
```

### Comentários

- **GetItems**: Método GET que recebe os parâmetros de paginação e retorna os itens paginados.
- **paginationParameters**: Parâmetros de paginação recebidos da query string.
- **pagedItems**: Itens paginados obtidos do repositório.
- **listaAlunosDto**: Mapeamento dos itens paginados para DTOs.

---

## Resumo

Este guia detalhou a implementação de paginação em uma aplicação ASP.NET Core, cobrindo as seguintes etapas:

1. Criação de uma classe abstrata genérica para parâmetros de paginação.
2. Herança da classe genérica para casos específicos.
3. Implementação de uma classe `PagedList<T>` para encapsular a lógica de paginação.
4. Ajuste e implementação de métodos de paginação no repositório.
5. Criação de um método no controller para utilizar a paginação.

Seguindo essas etapas, você poderá adicionar paginação facilmente a qualquer conjunto de dados em sua aplicação, melhorando a performance e a experiência do usuário.