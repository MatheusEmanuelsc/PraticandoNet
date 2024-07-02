# Tutorial sobre Controladores na API

## Índice

1. [Introdução aos Controladores](#introdução-aos-controladores)
2. [Estrutura do Controlador](#estrutura-do-controlador)
    - [Exemplo de Criação de Controlador](#exemplo-de-criação-de-controlador)
3. [Injeção de Dependência](#injeção-de-dependência)
4. [Criação dos Endpoints](#criação-dos-endpoints)
    - [GET](#get)
        - [Lista de Categorias](#lista-de-categorias)
        - [Categoria por ID](#categoria-por-id)
        - [Categorias com Produtos](#categorias-com-produtos)
    - [POST](#post)
        - [Criação de Categoria](#criação-de-categoria)
    - [PUT](#put)
        - [Atualização de Categoria](#atualização-de-categoria)
    - [DELETE](#delete)
        - [Deletar Categoria](#deletar-categoria)
5. [Considerações Finais](#considerações-finais)

## Introdução aos Controladores

Os controladores são os cérebros da API, responsáveis por processar as requisições e enviar respostas apropriadas. É uma boa prática nomear os controladores com o sufixo `Controller`.

## Estrutura do Controlador

Crie uma pasta `Controllers` e adicione o controlador dentro dela.

### Exemplo de Criação de Controlador

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProdutoController : ControllerBase
{
}
```

## Injeção de Dependência

Nesta parte, vamos configurar a injeção de dependência, incluindo o banco de dados.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProdutoController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutoController(AppDbContext context)
    {
        _context = context;
    }
}
```

> **Nota:** Se estiver utilizando Visual Studio, você pode gerar automaticamente o template com os métodos, além de outras opções.

## Criação dos Endpoints

Nesta etapa, criaremos os endpoints. Para seguir o padrão RESTful, utilizaremos os verbos HTTP:

- **GET**: Retorna dados.
- **POST**: Cria dados.
- **PUT**: Atualiza dados.
- **DELETE**: Deleta dados.

Os métodos devem ser informados utilizando `[Http+Método]`. Ex: `[HttpGet]`.

### GET

#### Lista de Categorias

```csharp
[HttpGet]
public ActionResult<IEnumerable<Categoria>> ListaCategorias()
{
    try
    {
        var categorias = _context.Categorias.AsNoTracking().ToList();
        if (categorias is null)
        {
            return NotFound("Categoria não encontrada");
        }
        return categorias;
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:** 

- Utilizamos o método `AsNoTracking()` para desativar o rastreamento das entidades, o que melhora a performance em operações de leitura.
- O método retorna uma lista de categorias. Caso a lista esteja vazia, retorna um `NotFound`.
- Em caso de exceção, retorna um erro 500.

#### Categoria por ID

```csharp
[HttpGet("{id:int}", Name = "ObterCategoria")]
public ActionResult<Categoria> CategoriaPorId(int id)
{
    try
    {
        var categoria = _context.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaId == id);

        if (categoria is null)
        {
            return NotFound("Categoria não encontrada");
        }
        return categoria;
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:**

- Utiliza `{id:int}` para garantir que o parâmetro é um inteiro.
- O método `Name = "ObterCategoria"` permite que outras ações referenciem esta rota.
- Verifica se a categoria existe. Caso contrário, retorna um `NotFound`.
- Em caso de exceção, retorna um erro 500.

#### Categorias com Produtos

```csharp
[HttpGet("produtos")]
public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
{
    try
    {
        return _context.Categorias.AsNoTracking().Include(p => p.Produtos).Where(c => c.CategoriaId <= 5).ToList();
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:**

- Utiliza o método `Include` para carregar as entidades relacionadas (`Produtos`).
- Limita a busca a categorias com `CategoriaId` menor ou igual a 5.
- Em caso de exceção, retorna um erro 500.

**Nota:** Para evitar problemas de referência cíclica, adicione a seguinte configuração na classe `Program`:

```csharp
builder.Services.AddControllers()
  .AddJsonOptions(options =>
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
```

### POST

#### Criação de Categoria

```csharp
[HttpPost]
public ActionResult<Categoria> CriandoCategoria(Categoria categoria)
{
    try
    {
        _context.Categorias.Add(categoria);
        _context.SaveChanges();

        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:**

- Utiliza `_context.Categorias.Add(categoria)` para adicionar uma nova categoria.
- Utiliza `_context.SaveChanges()` para salvar as mudanças no banco de dados.
- O método `CreatedAtRouteResult` retorna um status 201 (Created) e chama a rota "ObterCategoria" passando o ID da nova categoria criada.

### PUT

#### Atualização de Categoria

```csharp
[HttpPut("{id:int}")]
public ActionResult Put(int id, Categoria categoria)
{
    try
    {
        if (id != categoria.CategoriaId)
        {
            return BadRequest();
        }

        _context.Entry(categoria).State = EntityState.Modified;
        _context.SaveChanges();

        return Ok(categoria);
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:**

- Verifica se o ID da URL corresponde ao ID da categoria. Se não, retorna `BadRequest`.
- Marca o estado da categoria como modificado e salva as mudanças.
- Retorna um status 200 (OK) com a categoria atualizada.

### DELETE

#### Deletar Categoria

```csharp
[HttpDelete("{id:int}")]
public ActionResult Delete(int id)
{
    try
    {
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
        if (categoria is null)
        {
            return NotFound("Categoria não encontrada");
        }

        _context.Categorias.Remove(categoria);
        _context.SaveChanges();
        return Ok(categoria);
    }
    catch (Exception)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação.");
    }
}
```

**Explicação:**

- Verifica se a categoria existe. Se não, retorna `NotFound`.
- Remove a categoria do banco de dados e salva as mudanças.
- Retorna um status 200 (OK) com a categoria deletada.

## Considerações Finais

- Sempre utilize o método `try-catch` para tratar exceções.
- Utilize `AsNoTracking` para otimizar consultas de leitura.
- Adicione o tratamento de referências cíclicas no método `AddJsonOptions` na configuração do `builder`.

```csharp
builder.Services.AddControllers()
  .AddJsonOptions(options =>
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
```

- Para ignorar propriedades na serialização, utilize `[JsonIgnore]` nas classes do modelo.