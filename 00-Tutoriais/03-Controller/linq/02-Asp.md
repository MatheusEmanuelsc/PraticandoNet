
```markdown
# Resumo de Uso do LINQ no ASP.NET Core (com Include)

## Índice
1. [Configuração Inicial](#configuração-inicial)
2. [Métodos de Filtragem](#métodos-de-filtragem)
3. [Métodos de Projeção](#métodos-de-projeção)
4. [Métodos de Ordenação](#métodos-de-ordenação)
5. [Métodos de Agregação](#métodos-de-agregação)
6. [Métodos de Junção](#métodos-de-junção)
7. [Métodos de Conjunto](#métodos-de-conjunto)
8. [Métodos de Quantificação](#métodos-de-quantificação)
9. [Métodos de Elementos](#métodos-de-elementos)
10. [Métodos de Geração](#métodos-de-geração)
11. [Método Include](#método-include)

---

## Configuração Inicial
No ASP.NET Core, o LINQ é frequentemente usado com o **Entity Framework Core (EF Core)** para consultar bancos de dados ou com coleções em memória (como `List<T>`). Para usar com EF Core:
1. Configure um `DbContext` no `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```
2. Injete o `DbContext` em um controlador ou serviço:
   ```csharp
   private readonly AppDbContext _context;
   public MeuController(AppDbContext context) => _context = context;
   ```

---

## Métodos de Filtragem
- **Where**: Filtra dados de uma tabela ou coleção.
  ```csharp
  [HttpGet("ativos")]
  public IActionResult GetAtivos() => Ok(_context.Produtos.Where(p => p.Ativo).ToList());
  ```

## Métodos de Projeção
- **Select**: Transforma resultados em DTOs.
  ```csharp
  [HttpGet]
  public IActionResult GetProdutos() => Ok(_context.Produtos.Select(p => new { p.Id, p.Nome }).ToList());
  ```

## Métodos de Ordenação
- **OrderBy**: Ordena resultados.
  ```csharp
  [HttpGet("ordenados")]
  public IActionResult GetOrdenados() => Ok(_context.Produtos.OrderBy(p => p.Preco).ToList());
  ```

## Métodos de Agregação
- **Count**: Conta registros.
  ```csharp
  [HttpGet("total")]
  public IActionResult GetTotal() => Ok(_context.Produtos.Count());
  ```

## Métodos de Junção
- **Join**: Combina tabelas relacionadas.
  ```csharp
  var resultado = _context.Pedidos
      .Join(_context.Clientes, p => p.ClienteId, c => c.Id, (p, c) => new { p.Id, c.Nome })
      .ToList();
  ```

## Métodos de Conjunto
- **Distinct**: Remove duplicatas.
  ```csharp
  var categorias = _context.Produtos.Select(p => p.Categoria).Distinct().ToList();
  ```

## Métodos de Quantificação
- **Any**: Verifica existência.
  ```csharp
  [HttpGet("tem-estoque")]
  public IActionResult TemEstoque() => Ok(_context.Produtos.Any(p => p.Estoque > 0));
  ```

## Métodos de Elementos
- **FirstOrDefault**: Busca um registro.
  ```csharp
  [HttpGet("{id}")]
  public IActionResult GetPorId(int id) => Ok(_context.Produtos.FirstOrDefault(p => p.Id == id));
  ```

## Métodos de Geração
- **Range**: Gera dados para testes.
  ```csharp
  var paginas = Enumerable.Range(1, 10).ToList();
  ```

## Método Include
- **Include**: Carrega dados relacionados de forma antecipada (eager loading) no EF Core. Útil para evitar consultas N+1 e trazer entidades associadas em uma única query.
  ```csharp
  [HttpGet("pedidos-com-clientes")]
  public async Task<IActionResult> GetPedidosComClientes()
  {
      var pedidos = await _context.Pedidos
          .Include(p => p.Cliente) // Carrega a entidade Cliente relacionada
          .Include(p => p.Itens)   // Carrega a coleção de Itens do pedido
          .ToListAsync();
      return Ok(pedidos);
  }
  ```
  - **ThenInclude**: Usado para carregar níveis adicionais de relacionamento:
    ```csharp
    var pedidos = await _context.Pedidos
        .Include(p => p.Cliente)
        .ThenInclude(c => c.Enderecos) // Carrega os endereços do cliente
        .ToListAsync();
    ```
  - **Uso**: Ideal para APIs que precisam retornar dados completos (ex.: pedido com cliente e itens). Requer o pacote `Microsoft.EntityFrameworkCore`.
  - **Atenção**: Evite overuse para não carregar dados desnecessários, impactando a performance.

---

### Considerações no ASP.NET Core
- **EF Core**: Use `.ToListAsync()` com métodos assíncronos.
- **Include**: Certifique-se de que as propriedades de navegação (ex.: `Cliente`, `Itens`) estejam definidas no modelo e mapeadas no `DbContext`.
- **Performance**: Combine `Include` com `Where` ou `Select` para limitar os dados carregados.
- **Alternativas**: Para cenários complexos, considere **lazy loading** (carregamento tardio) ou **explicit loading** (carregamento explícito) em vez de `Include`.
```

### Explicação sobre o `Include`
O `Include` é uma extensão do EF Core que instrui o provedor de banco de dados a incluir entidades relacionadas na consulta SQL gerada. Por exemplo, ao buscar um `Pedido`, você pode usar `Include(p => p.Cliente)` para trazer os dados do cliente associado em vez de deixar como `null` ou exigir uma consulta separada. Ele é particularmente útil em APIs REST do ASP.NET Core, onde você precisa retornar objetos completos em uma única resposta. O `ThenInclude` permite aprofundar ainda mais nos relacionamentos, como acessar propriedades de uma entidade já incluída.