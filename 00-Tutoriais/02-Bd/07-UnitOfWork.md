# Unit of Work em Web API

## Índice
1. [O que é o padrão Unit of Work?](#o-que-é-o-padrão-unit-of-work)
2. [Benefícios do Unit of Work](#benefícios-do-unit-of-work)
3. [Implementação do Unit of Work](#implementação-do-unit-of-work)
   - [Definição da Interface IUnitOfWork](#definição-da-interface-iunitofwork)
   - [Implementação da Classe UnitOfWork](#implementação-da-classe-unitofwork)
   - [Uso do Unit of Work no Service e Controller](#uso-do-unit-of-work-no-service-e-controller)
4. [Resumo](#resumo)

## O que é o padrão Unit of Work?
O **Unit of Work (UoW)** é um padrão de design que mantém o controle de operações de banco de dados dentro de uma única transação, garantindo que todas as mudanças sejam aplicadas ou revertidas juntas.

## Benefícios do Unit of Work
- **Atomicidade**: Garante que todas as operações sejam concluídas com sucesso ou revertidas.
- **Melhor desempenho**: Reduz o número de chamadas ao banco de dados.
- **Facilidade de manutenção**: Centraliza a lógica de transação em um único local.
- **Desacoplamento**: Facilita testes e substituição de repositórios.

## Implementação do Unit of Work
### Definição da Interface IUnitOfWork
```csharp
public interface IUnitOfWork : IDisposable
{
    IProdutoRepository Produtos { get; }
    ICategoriaRepository Categorias { get; }
    Task<int> CommitAsync();
}
```
**Explicação:**
- Define repositórios acessíveis pelo Unit of Work.
- `CommitAsync()` aplica todas as operações em uma única transação.
- `IDisposable` permite liberar recursos corretamente.

### Implementação da Classe UnitOfWork
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IProdutoRepository Produtos { get; }
    public ICategoriaRepository Categorias { get; }

    public UnitOfWork(AppDbContext context, IProdutoRepository produtos, ICategoriaRepository categorias)
    {
        _context = context;
        Produtos = produtos;
        Categorias = categorias;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```
**Explicação:**
- Mantém referência ao `AppDbContext` e aos repositórios.
- `CommitAsync()` chama `SaveChangesAsync()` para persistir todas as mudanças.
- `Dispose()` libera os recursos do contexto.

### Uso do Unit of Work no Service e Controller
#### Service
```csharp
public class ProdutoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> AdicionarProduto(Produto produto)
    {
        _unitOfWork.Produtos.Add(produto);
        return await _unitOfWork.CommitAsync() > 0;
    }
}
```
#### Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _produtoService;

    public ProdutosController(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpPost]
    public async Task<IActionResult> CriarProduto([FromBody] Produto produto)
    {
        if (await _produtoService.AdicionarProduto(produto))
            return Ok();
        return BadRequest();
    }
}
```
**Explicação:**
- `ProdutoService` usa `UnitOfWork` para gerenciar repositórios.
- `ProdutosController` injeta `ProdutoService` para manipular requisições HTTP.

## Resumo
O padrão **Unit of Work** permite que múltiplas operações no banco de dados sejam gerenciadas em uma única transação. Isso reduz chamadas ao banco, melhora a manutenção do código e garante a atomicidade das operações. Ele é comumente implementado em Web APIs usando repositórios e um contexto de banco de dados, como o `DbContext` no Entity Framework Core.
