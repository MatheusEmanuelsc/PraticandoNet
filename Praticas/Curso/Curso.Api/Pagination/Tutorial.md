
etapa  1

    primeiro cria uma classe genrica e bastrata


    namespace APICatalogo.Pagination
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

etapa 2
        
        depois faça herda  ela pra qualsses vc quer  por apaginação vc tbm pode adc metodos neles

        public class ProdutosParameters : PaginationParameters
{
    // Adicione qualquer outro parâmetro específico para a paginação de produtos, se necessário
}

etapa 3

    para iniciar apgainação vc vai criar uma nova classe que herda de list generica

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

    criei esse metodo aqui

   public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}


etapa 4

 depois de criar ela ao inves de utilizar IEnurable utilize

 public interface IProdutoRepository : IRepository<Produto>
{
    PagedList<Produto> GetProdutos(ProdutosParameters produtosParams);
    IEnumerable<Produto> GetProdutosPorCategoria(int id);
}