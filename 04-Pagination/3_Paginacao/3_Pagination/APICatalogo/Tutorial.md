para iniciar a paginação 

 primeiro vc vai criar uma classe aonde vc vai pos parametros

 public class ProdutosParameters
{
    const int maxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    private int _pageSize;
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize= (value > maxPageSize)? maxPageSize : value;
        }
    }    
}


depois adicione no repositorio

public interface IProdutoRepository : IRepository<Produto>
{
    IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams);  essa 


    
}


depois na implementação    adc

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context): base(context)
    {       
    }

    public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams)
    {
        return GetAll()
            .OrderBy(p => p.Nome)
            .Skip((produtosParams.PageNumber - 1) * produtosParams.PageSize)
            .Take(produtosParams.PageSize).ToList();

    }

    depopis so implementa no controller
}


como no exemplo

[HttpGet("pagination")]
    public ActionResult<IEnumerable<ProdutoDTO>> Get ([FromQuery] 
                                   ProdutosParameters produtosParameters)
    {
        var produtos = _uof.ProdutoRepository.GetProdutos(produtosParameters);

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }


etapa 2 vamos  incrementar agora

    