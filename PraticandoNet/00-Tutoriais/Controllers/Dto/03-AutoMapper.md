# Utilizando AutoMapper no .NET

O AutoMapper é uma biblioteca popular no .NET para mapear objetos de um tipo para outro. Ele é amplamente utilizado para transformar Data Transfer Objects (DTOs) em entidades de domínio e vice-versa, simplificando a conversão de dados entre diferentes camadas de uma aplicação. Abaixo está um guia detalhado sobre como configurar e usar o AutoMapper no .NET.

## 1. Adicionando o Pacote AutoMapper

Primeiro, é necessário adicionar o pacote AutoMapper ao seu projeto. Execute o seguinte comando:

```bash
dotnet add package AutoMapper
```

Se você estiver utilizando uma versão do .NET inferior à 6, será necessário também adicionar o pacote `AutoMapper.Extensions.Microsoft.DependencyInjection`:

```bash
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

## 2. Criando a Classe DTO

Crie a classe DTO para o objeto que deseja mapear. Por exemplo, a classe `ProdutoDTO`:

```csharp
public class ProdutoDTO
{
    public int ProdutoId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Nome { get; set; }

    [Required]
    [StringLength(300)]
    public string? Descricao { get; set; }

    [Required]
    public decimal Preco { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImagemUrl { get; set; }

    public int CategoriaId { get; set; }
}
```

## 3. Criando o Profile para Mapping

Em seguida, crie um perfil de mapeamento onde configurará os mapeamentos entre as entidades e os DTOs. Por exemplo:

```csharp
namespace APICatalogo.DTOs.Mappings
{
    public class ProdutoDTOMappingProfile : Profile
    {
        public ProdutoDTOMappingProfile()
        {
            CreateMap<Produto, ProdutoDTO>().ReverseMap();
            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
        }
    }
}
```

## 4. Ajustando o Controller

No controlador, injete o `IMapper` e utilize-o para mapear os objetos.

```csharp
public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ProdutosController(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    [HttpGet("produtos/{id}")]
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosCategoria(int id)
    {
        var produtos = _uof.ProdutoRepository.GetProdutosPorCategoria(id);

        if (produtos is null)
            return NotFound();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _uof.ProdutoRepository.GetAll();
        if (produtos is null)
        {
            return NotFound();
        }
        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
        return Ok(produtosDto);
    }

    [HttpGet("{id}", Name = "ObterProduto")]
    public ActionResult<ProdutoDTO> Get(int id)
    {
        var produto = _uof.ProdutoRepository.Get(c => c.ProdutoId == id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return Ok(produtoDto);
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
    {
        if (produtoDto is null)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto);

        var novoProduto = _uof.ProdutoRepository.Create(produto);
        _uof.Commit();

        var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProdutoDto.ProdutoId }, novoProdutoDto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto);

        var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
        _uof.Commit();

        var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoAtualizado);

        return Ok(produtoAtualizadoDto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
        _uof.Commit();

        var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);

        return Ok(produtoDeletadoDto);
    }
}
```

## 5. Configurando o AutoMapper no Program

Adicione o AutoMapper aos serviços da aplicação no arquivo `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionando AutoMapper
builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));

// Outros serviços...

var app = builder.Build();

// Configuração do middleware...

app.Run();
```

Com essas etapas, o AutoMapper estará configurado e pronto para uso em sua aplicação .NET, permitindo a conversão automática entre entidades e DTOs de forma eficiente.