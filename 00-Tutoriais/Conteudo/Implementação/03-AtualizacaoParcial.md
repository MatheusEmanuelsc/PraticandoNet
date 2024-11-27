```markdown
# Tutorial de Atualização Parcial em API ASP.NET Core utilizando JSON Patch

## Índice

- [Introdução](#introdução)
- [Etapa 1: Adicionar Pacotes Necessários](#etapa-1-adicionar-pacotes-necessários)
- [Etapa 2: Configurar o Program](#etapa-2-configurar-o-program)
- [Etapa 3: Criar os DTOs de Request e Response](#etapa-3-criar-os-dtos-de-request-e-response)
- [Etapa 4: Configurar o AutoMapper](#etapa-4-configurar-o-automapper)
- [Etapa 5: Criar a Requisição no Controlador](#etapa-5-criar-a-requisição-no-controlador)
- [Resumo](#resumo)

## Introdução

Neste tutorial, vamos aprender como implementar atualizações parciais em uma API ASP.NET Core utilizando JSON Patch. As etapas incluem adicionar pacotes necessários, configurar o projeto, criar DTOs de request e response, validar as entradas, configurar o AutoMapper e adicionar a ação PATCH ao controlador.

## Etapa 1: Adicionar Pacotes Necessários

Primeiro, adicione os seguintes pacotes ao seu projeto:

```sh
dotnet add package Microsoft.AspNetCore.JsonPatch
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
```

## Etapa 2: Configurar o Program

Configure o `Program.cs` para adicionar os serviços necessários:

```csharp
// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
})
.AddNewtonsoftJson();
```

Se você já tiver implementado o `ApiExceptionFilter` e deseja ignorá-lo, basta adicionar:

```csharp
.AddNewtonsoftJson();
```

## Etapa 3: Criar os DTOs de Request e Response

Crie os DTOs de request e response no namespace `APICatalogo.DTOs`:

```csharp
namespace APICatalogo.DTOs
{
    public class ProdutoDTOUpdateRequest
    {
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}

namespace APICatalogo.DTOs
{
    public class ProdutoDTOUpdateResponse
    {
        public int ProdutoId { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? ImagemUrl { get; set; }
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
        public int CategoriaId { get; set; }
    }
}
```

Opcionalmente, você pode adicionar validação ao `ProdutoDTOUpdateRequest`:

```csharp
namespace APICatalogo.DTOs
{
    public class ProdutoDTOUpdateRequest : IValidatableObject
    {
        [Range(1, 9999, ErrorMessage = "Estoque deve estar entre 1 e 9999")]
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataCadastro <= DateTime.Now.Date)
            {
                yield return new ValidationResult("A data deve ser maior que a data atual", new[] { nameof(this.DataCadastro) });
            }
        }
    }
}
```

## Etapa 4: Configurar o AutoMapper

Configure o AutoMapper:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Produto, ProdutoDTOUpdateRequest>().ReverseMap();
        CreateMap<Produto, ProdutoDTOUpdateResponse>().ReverseMap();
    }
}
```

## Etapa 5: Criar a Requisição no Controlador

Adicione a ação PATCH ao seu controlador:

```csharp
[HttpPatch("{id}/UpdatePartial")]
public ActionResult<ProdutoDTOUpdateResponse> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> pathProdutoDto)
{
    if (pathProdutoDto is null || id <= 0)
    {
        return BadRequest();
    }

    var produto = _uof.ProdutoRepository.Get(c => c.ProdutoId == id);
    if (produto is null)
    {
        return NotFound();
    }

    var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);
    pathProdutoDto.ApplyTo(produtoUpdateRequest, ModelState);

    if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
    {
        return BadRequest();
    }

    _mapper.Map(produtoUpdateRequest, produto);
    _uof.Commit();
    
    return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
}
```

### Comentários Explicativos

- **Verifica se o documento de patch é nulo ou se o ID é inválido:** Verifica se o documento de patch fornecido na requisição é nulo ou se o ID do produto é inválido.
- **Obtém o produto do repositório usando o ID fornecido:** Busca o produto no repositório com base no ID fornecido.
- **Verifica se o produto foi encontrado:** Verifica se o produto existe no repositório.
- **Mapeia o produto para um DTO de atualização:** Converte o produto para um DTO de atualização.
- **Aplica o documento de patch ao DTO de atualização:** Aplica as alterações do documento de patch ao DTO de atualização.
- **Verifica se o modelo é válido:** Verifica se o modelo resultante é válido.
- **Mapeia o DTO de atualização de volta para o modelo de produto:** Converte o DTO de atualização de volta para o modelo de produto.
- **Salva as alterações no repositório:** Salva as alterações feitas no repositório.
- **Mapeia o produto atualizado para um DTO de resposta e retorna:** Converte o produto atualizado para um DTO de resposta e retorna.

## Resumo

Neste tutorial, você aprendeu como implementar atualizações parciais em uma API ASP.NET Core utilizando JSON Patch. As etapas incluíram adicionar pacotes necessários, configurar o projeto, criar DTOs de request e response, validar as entradas, configurar o AutoMapper e adicionar a ação PATCH ao controlador. Com isso, você pode realizar atualizações parciais em objetos complexos de forma eficiente e segura.
```