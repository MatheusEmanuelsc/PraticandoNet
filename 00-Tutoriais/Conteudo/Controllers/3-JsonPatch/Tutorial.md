```md
# Tutorial de Atualização Parcial com ASP.NET Core

## Índice

1. [Etapa 1: Adicionar Pacotes](#etapa-1-adicionar-pacotes)
2. [Etapa 2: Configurar o Program](#etapa-2-configurar-o-program)
3. [Etapa 3: Criar DTOs de Request e Response](#etapa-3-criar-dtos-de-request-e-response)
4. [Etapa 4: Validar as Informações de Entrada](#etapa-4-validar-as-informações-de-entrada)
5. [Etapa 5: Configurar AutoMapper](#etapa-5-configurar-automapper)
6. [Etapa 6: Criar a Requisição no Controlador](#etapa-6-criar-a-requisição-no-controlador)
7. [Resumo](#resumo)

## Etapa 1: Adicionar Pacotes

Adicione os seguintes pacotes ao seu projeto:

```bash
dotnet add package Microsoft.AspNetCore.JsonPatch
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
```

- **Microsoft.AspNetCore.JsonPatch**: Permite implementar operações de patch parcial em recursos RESTful por meio do método PATCH. Oferece suporte a atualizações parciais em objetos complexos como modelos de domínio ou entidades de banco de dados.
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson**: Habilita o suporte ao JSON Patch no ASP.NET Core Web API. Fornece um parser e um serializador para o formato JSON Patch.

## Etapa 2: Configurar o Program

Adicione a configuração abaixo no método `Program` do seu projeto:

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

- **AddControllers**: Adiciona serviços de controle ao contêiner de serviços.
- **options.Filters.Add**: Adiciona um filtro global de exceções.
- **AddJsonOptions**: Configura opções para o serializador JSON.
- **ReferenceHandler.IgnoreCycles**: Ignora referências cíclicas.
- **AddNewtonsoftJson**: Adiciona suporte para JSON Patch com Newtonsoft.Json.

## Etapa 3: Criar DTOs de Request e Response

### Modelo de Exemplo

```csharp
namespace APICatalogo.Models
{
    [Table("Produtos")]
    public class Produto
    {
        [Key]
        public int ProdutoId { get; set; }

        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? Descricao { get; set; }
        
        [Required]
        [Column(TypeName ="decimal(10,2)")]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
        public int CategoriaId { get; set; }

        [JsonIgnore]
        public Categoria? Categoria { get; set; }
    }
}
```

### DTO de Request

Crie um DTO de request especificando as propriedades que deseja atualizar:

```csharp
namespace APICatalogo.DTOs
{
    public class ProdutoDTOUpdateRequest
    {
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
```

### DTO de Response

Crie um DTO de response com todas as propriedades do modelo:

```csharp
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

## Etapa 4: Validar as Informações de Entrada

Adicione validações ao DTO de request:

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

## Etapa 5: Configurar AutoMapper

Configure o AutoMapper para mapear entre os modelos e os DTOs:

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

## Etapa 6: Criar a Requisição no Controlador

Adicione a ação PATCH ao seu controlador:

```csharp
[HttpPatch("{id}/UpdatePartial")]
public ActionResult<ProdutoDTOUpdateResponse> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> pathProdutoDto)
{
    // Verifica se o documento de patch é nulo ou se o ID é inválido
    if (pathProdutoDto is null || id <= 0)
    {
        return BadRequest();
    }

    // Obtém o produto do repositório usando o ID fornecido
    var produto = _uof.ProdutoRepository.Get(c => c.ProdutoId == id);
    
    // Verifica se o produto foi encontrado
    if (produto is null)
    {
        return NotFound();
    }

    // Mapeia o produto para um DTO de atualização
    var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);

    // Aplica o documento de patch ao DTO de atualização
    pathProdutoDto.ApplyTo(produtoUpdateRequest, ModelState);

    // Verifica se o modelo é válido
    if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
    {
        return BadRequest();
    }

    // Mapeia o DTO de atualização de volta para o modelo de produto
    _mapper.Map(produtoUpdateRequest, produto);
    
    // Salva as alterações no repositório
    _uof.Commit();
    
    // Mapeia o produto atualizado para um DTO de resposta e retorna
    return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
}
```

### Comentários Explicativos

- **Verifica se o documento de patch é nulo ou se o ID é inválido**: Verifica se o documento de patch fornecido na requisição é nulo ou se o ID do produto é inválido.
- **Obtém o produto do repositório usando o ID fornecido**: Busca o produto no repositório com base no ID fornecido.
- **Verifica se o produto foi encontrado**: Verifica se o produto existe no repositório.
- **Mapeia o produto para um DTO de atualização**: Converte o produto para um DTO de atualização.
- **Aplica o documento de patch ao DTO de atualização**: Aplica as alterações do documento de patch ao DTO de atualização.
- **Verifica se o modelo é válido**: Verifica se o modelo resultante é válido.
- **Mapeia o DTO de atualização de volta para o modelo de produto**: Converte o DTO de atualização de volta para o modelo de produto.
- **Salva as alterações no repositório**: Salva as alterações feitas no repositório.
- **Mapeia o produto atualizado para um DTO de resposta e retorna**: Converte o produto atualizado para um DTO de resposta e retorna.

## Resumo

Neste tutorial, você aprendeu como implementar atualizações parciais em uma API ASP.NET Core utilizando JSON Patch. As etapas incluíram adicionar pacotes necessários, configurar o projeto, criar DTOs de request e response, validar as entradas, configurar o AutoMapper e adicionar a ação PATCH ao controlador. Com isso, você pode realizar atualizações parciais em objetos complexos de forma eficiente e segura.
```

### Exemplo Adicional

Vamos adicionar um exemplo adicional para ilustrar melhor o processo.

### Novo Modelo de Exemplo

```csharp
namespace APICatalogo.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(200)]
        public string? Email { get; set; }

        public DateTime DataNascimento { get; set; }
    }
}
```

### Novo DTO de Request

```csharp
namespace APICatalogo.DTOs
{
    public class UsuarioDTOUpdateRequest
    {
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}
```



### Novo DTO de Response

```csharp
namespace APICatalogo.DTOs
{
    public class UsuarioDTOUpdateResponse
    {
        public int UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}
```

### Novo Validador

```csharp
namespace APICatalogo.DTOs
{
    public class UsuarioDTOUpdateRequest : IValidatableObject
    {
        [EmailAddress(ErrorMessage = "O formato do email é inválido")]
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataNascimento >= DateTime.Now.Date)
            {
                yield return new ValidationResult("A data de nascimento deve ser menor que a data atual", new[] { nameof(this.DataNascimento) });
            }
        }
    }
}
```

### Novo Configurador de AutoMapper

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDTOUpdateRequest>().ReverseMap();
        CreateMap<Usuario, UsuarioDTOUpdateResponse>().ReverseMap();           
    }
}
```

### Novo Controlador

```csharp
[HttpPatch("{id}/UpdatePartial")]
public ActionResult<UsuarioDTOUpdateResponse> Patch(int id, JsonPatchDocument<UsuarioDTOUpdateRequest> pathUsuarioDto)
{
    if (pathUsuarioDto is null || id <= 0)
    {
        return BadRequest();
    }

    var usuario = _uof.UsuarioRepository.Get(c => c.UsuarioId == id);
    if (usuario is null)
    {
        return NotFound();
    }

    var usuarioUpdateRequest = _mapper.Map<UsuarioDTOUpdateRequest>(usuario);
    pathUsuarioDto.ApplyTo(usuarioUpdateRequest, ModelState);

    if (!ModelState.IsValid || !TryValidateModel(usuarioUpdateRequest))
    {
        return BadRequest();
    }

    _mapper.Map(usuarioUpdateRequest, usuario);
    _uof.Commit();
    return Ok(_mapper.Map<UsuarioDTOUpdateResponse>(usuario));
}
```

Este exemplo adicional demonstra como você pode aplicar o mesmo processo a diferentes modelos, criando DTOs personalizados, validando entradas e configurando AutoMapper para gerenciar as atualizações parciais de forma eficiente.
```