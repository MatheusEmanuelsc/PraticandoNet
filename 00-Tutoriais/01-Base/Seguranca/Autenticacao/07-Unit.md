Ótimo! Vamos fazer isso muito bem feito: integrar **IdentityDbContext** + **ApplicationDbContext** com **Unit of Work (UoW)** + **Repository Pattern** no ASP.NET Core 8. 🚀

Aqui está o material completo organizado:

---

# 📄 Integrar IdentityDbContext + ApplicationDbContext ao Unit of Work (UoW)

---

## 📚 Contexto
Quando usamos **ASP.NET Core Identity**, o próprio Identity já gerencia o `IdentityDbContext`, que é separado do seu banco de dados principal (`ApplicationDbContext`).

Se estamos usando **Unit of Work e Repositories**, precisamos:
- Ter o `ApplicationDbContext` para as tabelas da aplicação.
- Ter o `IdentityDbContext` para usuários, roles, claims, etc.
- Conseguir transacionar entre eles se necessário (em algumas operações).
- E manter o projeto organizado, sem misturar responsabilidade.

---

# 🏗️ Estrutura esperada

```plaintext
/Data
  /Contexts
    - ApplicationDbContext.cs
    - IdentityDbContext.cs
  /Interfaces
    - IRepository.cs
    - IUnitOfWork.cs
  /Repositories
    - Repository.cs
    - UnitOfWork.cs
/Models
  - Produto.cs
  - Categoria.cs
/Identity
  - ApplicationUser.cs
  - ApplicationRole.cs
/Services
  - AuthService.cs
```

---

# 1. 🛠️ Definir os dois DbContexts

### `ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
}
```

### `IdentityDbContext.cs`

```csharp
public class IdentityDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }
}
```

> `ApplicationUser` e `ApplicationRole` são suas classes customizadas que herdam de `IdentityUser` e `IdentityRole`.

---

# 2. 🛠️ Interface do UnitOfWork

### `IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    IProdutoRepository Produtos { get; }
    ICategoriaRepository Categorias { get; }
    
    Task<int> CommitAsync();
}
```

---

# 3. 🛠️ Implementação do UnitOfWork

### `UnitOfWork.cs`

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IProdutoRepository _produtos;
    public IProdutoRepository Produtos => _produtos ??= new ProdutoRepository(_context);

    private ICategoriaRepository _categorias;
    public ICategoriaRepository Categorias => _categorias ??= new CategoriaRepository(_context);

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

✅ **Importante**: O UnitOfWork gerencia apenas **ApplicationDbContext**, porque o `IdentityDbContext` é gerenciado internamente pelo ASP.NET Identity.

---

# 4. 🛠️ Identity separado: UserManager / RoleManager

Para mexer com usuários e roles, você usa **services próprios do Identity**, como:

```csharp
private readonly UserManager<ApplicationUser> _userManager;
private readonly RoleManager<ApplicationRole> _roleManager;
```

Esses são injetados normalmente via **DI (Dependency Injection)** e gerenciam o `IdentityDbContext`.

---

# 5. 🛠️ Configurar no `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationConnection")));

// IdentityDbContext
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Identity Services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// UserManager, RoleManager são adicionados automaticamente pelo AddIdentity()
```

**Observação:**  
Se você quiser, pode usar **o mesmo banco** para ApplicationDbContext e IdentityDbContext — mas **é mais organizado separá-los** para projetos maiores.

---

# ⚙️ Exemplo prático de uso no Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ProdutosController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetProdutos()
    {
        var produtos = await _uow.Produtos.GetAllAsync();
        return Ok(produtos);
    }
}
```

E para autenticação:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
            return Ok();

        return BadRequest(result.Errors);
    }
}
```

---

# 📋 Resumo Rápido

- ✅ `ApplicationDbContext` gerencia tabelas da aplicação (Produtos, Categorias, etc.).
- ✅ `IdentityDbContext` gerencia usuários, roles, claims.
- ✅ `UnitOfWork` gerencia apenas `ApplicationDbContext`.
- ✅ Usuários/Roles são gerenciados via `UserManager` e `RoleManager`.
- ✅ Cada contexto tem sua própria connection string (opcional).

---

# 🔥 Quer deixar ainda mais avançado?

Se quiser, eu também posso mostrar como **transacionar os dois contextos juntos** (caso precise fazer algo muito crítico envolvendo Application + Identity no mesmo momento) usando **TransactionScope**!  
Quer que eu monte esse bônus também? 🚀🎯