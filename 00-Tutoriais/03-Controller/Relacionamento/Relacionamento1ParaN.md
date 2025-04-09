# 📘 ASP.NET Core 8 - Relacionamento 1:N com Recurso Dependente

## 📑 Índice

1. [Visão Geral](#visão-geral)
2. [Modelos (Entities)](#modelos-entities)
3. [Configuração do DbContext](#configura%C3%A7%C3%A3o-do-dbcontext)
4. [DTOs e AutoMapper](#dtos-e-automapper)
5. [Repository e Unit of Work (caso dependente)](#repository-e-unit-of-work-caso-dependente)
6. [Controllers](#controllers)
   - [Controlador Principal (resumo)](#controlador-principal-resumo)
   - [Controlador Dependente (detalhado)](#controlador-dependente-detalhado)
7. [Atualização Parcial (PATCH)](#atualiza%C3%A7%C3%A3o-parcial-patch)
8. [Conclusão](#conclus%C3%A3o)

---

## ✨ Visão Geral

Neste guia, vamos implementar um relacionamento **1:N (um para muitos)** no ASP.NET Core 8. O foco será no controlador do **recurso dependente**, ou seja, aquele que depende da chave estrangeira. Utilizaremos boas práticas com **DTOs**, **AutoMapper**, **FluentValidation**, **Repository Pattern** e **Unit of Work**.

> Exemplo: Um `Author` pode ter vários `Books`.

---

## 🧱 Modelos (Entities)

### `Author.cs`
```csharp
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navegação 1:N
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

### `Book.cs`
```csharp
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Chave estrangeira (FK)
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
```

> **Com isso, o EF Core já reconhece o relacionamento 1:N pela convenção.**

---

## 🛠️ Configuração do DbContext

No `AppDbContext.cs`:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId);
    }
}
```

> ✅ Neste caso, a configuração é opcional se os nomes seguirem a convenção.

---

## 🧾 DTOs e AutoMapper

### `BookDto.cs`
```csharp
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

### `CreateBookDto.cs`
```csharp
public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
}
```

### `MappingProfile.cs`
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateBookDto, Book>();
    }
}
```

---

## 📦 Repository e Unit of Work (caso dependente)

### `IBookRepository.cs`
```csharp
public interface IBookRepository
{
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
    Task<Book?> GetByIdAsync(int authorId, int bookId);
    Task AddAsync(Book book);
    void Remove(Book book);
}
```

> ✉️ Este repositório é focado em acesso aos `Books` de um `Author`, logo é dependente.

---

## 🎮 Controllers

### 🔹 Controlador Principal (resumo)

```csharp
[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    // GET /api/authors
    // GET /api/authors/1
    // POST /api/authors
    // etc.
}
```

### 🔸 Controlador Dependente (detalhado)

```csharp
[ApiController]
[Route("api/authors/{authorId:int}/books")]
public class AuthorBooksController : ControllerBase
{
    private readonly IBookRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AuthorBooksController(IBookRepository repo, IUnitOfWork uow, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
    }

    // GET /api/authors/1/books
    [HttpGet]
    public async Task<IActionResult> GetAll(int authorId)
    {
        var books = await _repo.GetBooksByAuthorAsync(authorId);
        return Ok(_mapper.Map<IEnumerable<BookDto>>(books));
    }

    // GET /api/authors/1/books/2
    [HttpGet("{bookId:int}")]
    public async Task<IActionResult> GetById(int authorId, int bookId)
    {
        var book = await _repo.GetByIdAsync(authorId, bookId);
        if (book is null) return NotFound();
        return Ok(_mapper.Map<BookDto>(book));
    }

    // POST /api/authors/1/books
    [HttpPost]
    public async Task<IActionResult> Create(int authorId, [FromBody] CreateBookDto dto)
    {
        var book = _mapper.Map<Book>(dto);
        book.AuthorId = authorId;

        await _repo.AddAsync(book);
        await _uow.CommitAsync();

        var result = _mapper.Map<BookDto>(book);
        return CreatedAtAction(nameof(GetById), new { authorId, bookId = result.Id }, result);
    }

    // DELETE /api/authors/1/books/2
    [HttpDelete("{bookId:int}")]
    public async Task<IActionResult> Delete(int authorId, int bookId)
    {
        var book = await _repo.GetByIdAsync(authorId, bookId);
        if (book is null) return NotFound();

        _repo.Remove(book);
        await _uow.CommitAsync();
        return NoContent();
    }
}
```

> ✅ Cada rota é aninhada ao autor (`/api/authors/{authorId}/books/...`)
>
> ✅ Garantimos que cada `Book` criado está vinculado a um `Author`

---

## 🔄 Atualização Parcial (PATCH)

Para aplicar PATCH em um recurso dependente:

- A rota continua sendo `/api/authors/{authorId}/books/{bookId}`
- A verificação deve garantir que o recurso realmente pertence ao pai
- A validação deve aplicar regras apenas aos campos alterados

```csharp
[HttpPatch("{bookId:int}")]
public async Task<IActionResult> Patch(int authorId, int bookId, [FromBody] JsonPatchDocument<Book> patchDoc)
{
    var book = await _repo.GetByIdAsync(authorId, bookId);
    if (book is null) return NotFound();

    patchDoc.ApplyTo(book);
    await _uow.CommitAsync();

    return NoContent();
}
```

> ⚠️ Em produção, use DTOs para aplicar o patch com validação (usando `FluentValidation`).

---

## ✅ Conclusão

- A relação 1:N é simples com o EF Core, mas é essencial entender como o recurso dependente deve ser tratado.
- Sempre use rotas aninhadas no controlador dependente.
- O uso de `DTO`, `AutoMapper`, `Repository` e `UnitOfWork` torna o código limpo, testável e organizado.
- No caso de `Books`, todas as operações são feitas dentro do contexto do `Author` (o recurso pai).



