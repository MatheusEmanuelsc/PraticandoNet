

# 📚 Fluent API no Entity Framework Core (.NET 8)

## Índice
- [O que é Fluent API?](#o-que-é-fluent-api)
- [Por que usar Fluent API?](#por-que-usar-fluent-api)
- [Onde usar Fluent API?](#onde-usar-fluent-api)
- [Configurações comuns via Fluent API](#configurações-comuns-via-fluent-api)
- [Fluent API x Data Annotations](#fluent-api-x-data-annotations)
- [Organizando Fluent API com IEntityTypeConfiguration](#organizando-fluent-api-com-ientitytypeconfiguration)
- [Exemplo completo](#exemplo-completo)
- [Resumo final](#resumo-final)

---

## 📖 O que é Fluent API?

Fluent API é a maneira de configurar modelos (entidades) no EF Core **usando código** em vez de atributos (`DataAnnotations`).

- É chamada de "Fluent" porque a configuração é encadeada ("flui").
- Permite mapear tabelas, colunas, restrições, relacionamentos, índices e muito mais.

> 🔥 **Importante**: Fluent API **já vem** por padrão no Entity Framework Core. Não precisa instalar nada além do pacote básico de EF.

---

## 🤔 Por que usar Fluent API?

- Melhor **separação** entre a regra de negócio (modelo) e a configuração do banco.
- Permite configurações avançadas que **não são possíveis** via atributos.
- Evita acoplamento entre EF Core e as classes de domínio.
- Facilita **testes** e **migração** futura para outros ORMs, se necessário.

---

## 📍 Onde usar Fluent API?

Você configura a Fluent API principalmente no método `OnModelCreating` do seu `DbContext`.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Produto>(entity =>
    {
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Nome).IsRequired().HasMaxLength(250);
    });
}
```

---
## 🛠️ Configurações comuns via Fluent API

| Tarefa                                | Exemplo                                                                                   |
|---------------------------------------|-------------------------------------------------------------------------------------------|
| Definir chave primária                | `entity.HasKey(e => e.Id);`                                                               |
| Definir campo obrigatório             | `entity.Property(e => e.Nome).IsRequired();`                                               |
| Definir tamanho máximo                | `entity.Property(e => e.Nome).HasMaxLength(250);`                                          |
| Definir nome da tabela                | `entity.ToTable("Produtos");`                                                             |
| Definir relacionamento 1:N            | `entity.HasMany(p => p.Itens).WithOne(i => i.Produto);`                                    |
| Definir relacionamento N:N (muitos)   | `modelBuilder.Entity<ProdutoCategoria>().HasKey(pc => new { pc.ProdutoId, pc.CategoriaId });` |
| Definir nome da coluna                | `entity.Property(e => e.Nome).HasColumnName("NomeProduto");`                               |
| Definir tipo de dado no banco         | `entity.Property(e => e.Preco).HasColumnType("decimal(18,2)");`                            |
| Criar índice                          | `entity.HasIndex(e => e.Email).IsUnique();`                                                |

---

## 📋 Fluent API x Data Annotations

| Critério                     | Fluent API                                  | Data Annotations                     |
|-------------------------------|---------------------------------------------|--------------------------------------|
| Local                         | `DbContext.OnModelCreating`                | Diretamente na entidade (modelo)     |
| Controle                      | Total (inclusive configurações avançadas)  | Limitado (apenas básico)             |
| Acoplamento com EF            | Não acopla                                 | Acopla                               |
| Recomendado para projetos...  | Médios e grandes                           | Pequenos e rápidos                   |
| Manutenção a longo prazo      | Melhor                                     | Pode dificultar                     |

---

## 📦 Organizando Fluent API com `IEntityTypeConfiguration<T>`

Para projetos maiores, é prática recomendada **separar as configurações** em arquivos individuais.

### 1. Criar uma classe de configuração:
```csharp
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(p => p.Preco)
               .HasColumnType("decimal(18,2)");
    }
}
```

### 2. Aplicar no `DbContext`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ProdutoConfiguration());
}
```

> 🔥 **Dica**: Use um arquivo separado para cada entidade (`ProdutoConfiguration.cs`, `CategoriaConfiguration.cs` etc.).

---

## 🚀 Exemplo completo

### Entidades:
```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }

    public ICollection<ProdutoCategoria> ProdutoCategorias { get; set; }
}

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; }

    public ICollection<ProdutoCategoria> ProdutoCategorias { get; set; }
}

public class ProdutoCategoria
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; }
}
```

### DbContext:
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(250);
            entity.Property(p => p.Preco).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<ProdutoCategoria>(entity =>
        {
            entity.ToTable("ProdutoCategorias");

            entity.HasKey(pc => new { pc.ProdutoId, pc.CategoriaId });

            entity.HasOne(pc => pc.Produto)
                  .WithMany(p => p.ProdutoCategorias)
                  .HasForeignKey(pc => pc.ProdutoId);

            entity.HasOne(pc => pc.Categoria)
                  .WithMany(c => c.ProdutoCategorias)
                  .HasForeignKey(pc => pc.CategoriaId);
        });
    }
}
```

---

## 📋 Resumo final

| Ponto                           | Detalhes |
|----------------------------------|----------|
| Fluent API já vem no EF Core     | Não precisa instalar nada a mais. |
| Onde usar                       | Método `OnModelCreating` no `DbContext`. |
| Melhor para projetos grandes    | Deixa a entidade limpa e a configuração separada. |
| Ideal usar `IEntityTypeConfiguration<T>` | Para organização e manutenção. |
| Ainda é possível fazer validações (Ex: MaxLength, Required) | Direto via Fluent API.|

---
