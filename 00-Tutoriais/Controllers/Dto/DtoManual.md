# Guia de Mapeamento Manual de DTO no .NET

Este guia fornece uma visão geral de como criar e mapear manualmente Objetos de Transferência de Dados (DTOs) em uma aplicação .NET. Os DTOs são utilizados para encapsular dados e transferi-los entre diferentes camadas de uma aplicação, garantindo que apenas os dados necessários sejam expostos.

## Índice
1. [Definição do Modelo](#definição-do-modelo)
2. [Criando DTOs](#criando-dtos)
3. [Extensões de Mapeamento](#extensões-de-mapeamento)
4. [Ajustes no Controller](#ajustes-no-controller)
5. [Visão Geral do Banco de Dados](#visão-geral-do-banco-de-dados)

## Definição do Modelo

Primeiro, definimos o modelo `Categoria` no namespace `APICatalogo.Models`. Este modelo representa a entidade que será armazenada no banco de dados.

```csharp
namespace APICatalogo.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        public Categoria()
        {
            Produtos = new Collection<Produto>();
        }

        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }

        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; }
    }
}
```

## Criando DTOs

Criamos uma classe DTO correspondente chamada `CategoriaDTO` em uma nova pasta chamada `Dto`. Esta classe espelha o modelo `Categoria`, mas é usada para transferir dados.

```csharp
namespace APICatalogo.Dto
{
    public class CategoriaDTO
    {
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }
    }
}
```

## Extensões de Mapeamento

Criamos métodos estáticos de mapeamento para converter entre `Categoria` e `CategoriaDTO`. Estes métodos garantem que os dados possam ser transferidos de maneira uniforme entre o modelo e o DTO.

```csharp
public static class CategoriaDTOMappingExtensions
{
    public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria)
    {
        if (categoria is null)
            return null;

        return new CategoriaDTO
        {
            CategoriaId = categoria.CategoriaId,
            Nome = categoria.Nome,
            ImagemUrl = categoria.ImagemUrl
        };
    }

    public static Categoria? ToCategoria(this CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null) return null;

        return new Categoria
        {
            CategoriaId = categoriaDto.CategoriaId,
            Nome = categoriaDto.Nome,
            ImagemUrl = categoriaDto.ImagemUrl
        };
    }

    public static IEnumerable<CategoriaDTO> ToCategoriaDTOList(this IEnumerable<Categoria> categorias)
    {
        if (categorias is null || !categorias.Any())
        {
            return new List<CategoriaDTO>();
        }

        return categorias.Select(categoria => new CategoriaDTO
        {
            CategoriaId = categoria.CategoriaId,
            Nome = categoria.Nome,
            ImagemUrl = categoria.ImagemUrl
        }).ToList();
    }
}
```

## Ajustes no Controller

Ajustamos o controller `CategoriasController` para utilizar os DTOs, aplicando os métodos de mapeamento nas ações do controller.

```csharp
namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger)
        {
            _logger = logger;
            _uof = uof;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoriaDTO>> Get()
        {
            var categorias = _uof.CategoriaRepository.GetAll();

            if (categorias is null)
                return NotFound("Não existem categorias...");

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);
        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<CategoriaDTO> Get(int id)
        {
            var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);

            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada...");
                return NotFound($"Categoria com id= {id} não encontrada...");
            }

            var categoriaDto = categoria.ToCategoriaDTO();

            return Ok(categoriaDto);
        }

        [HttpPost]
        public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                _logger.LogWarning($"Dados inválidos...");
                return BadRequest("Dados inválidos");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
            _uof.Commit();

            var novaCategoriaDto = categoriaCriada.ToCategoriaDTO();

            return new CreatedAtRouteResult("ObterCategoria",
                new { id = novaCategoriaDto.CategoriaId },
                novaCategoriaDto);
        }

        [HttpPut("{id:int}")]
        public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                _logger.LogWarning($"Dados inválidos...");
                return BadRequest("Dados inválidos");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
            _uof.Commit();

            var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDTO();

            return Ok(categoriaAtualizadaDto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<CategoriaDTO> Delete(int id)
        {
            var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);

            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id={id} não encontrada...");
                return NotFound($"Categoria com id={id} não encontrada...");
            }

            var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
            _uof.Commit();

            var categoriaExcluidaDto = categoriaExcluida.ToCategoriaDTO();

            return Ok(categoriaExcluidaDto);
        }
    }
}
```

## Visão Geral do Banco de Dados

O banco de dados deve conter uma tabela chamada `Categorias` que armazena as informações das categorias. As propriedades `CategoriaId`, `Nome` e `ImagemUrl` são os campos principais desta tabela. A propriedade `Produtos` é uma coleção de produtos associada a cada categoria, mas é ignorada durante a serialização JSON para evitar ciclos de referência.

---

Este guia cobre a criação e o mapeamento manual de DTOs em uma aplicação .NET, abordando desde a definição do modelo até a implementação no controller. Isso garante que os dados sejam transferidos de maneira eficiente e segura entre as diferentes camadas da aplicação.