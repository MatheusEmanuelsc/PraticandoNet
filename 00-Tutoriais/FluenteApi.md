

# Fluent API no Entity Framework Core

## Índice
1. [Introdução à Fluent API](#introducao)
2. [Por que usar Fluent API?](#porque-usar)
3. [Configuração da Fluent API](#configuracao)
4. [Principais Métodos e Configurações](#principais-metodos)
    - 4.1 [Configuração de Propriedades](#configuracao-propriedades)
    - 4.2 [Configuração de Tipos e Tamanhos](#configuracao-tipos)
    - 4.3 [Configuração de Chaves Primárias](#configuracao-chaves)
    - 4.4 [Configuração de Relacionamentos](#configuracao-relacionamentos)
    - 4.5 [Configuração de Índices](#configuracao-indices)
    - 4.6 [Configuração de Campos Computados](#configuracao-campos)
    - 4.7 [Configuração de Restrições e Regras](#configuracao-regras)
5. [Exemplos Práticos](#exemplos)
6. [Conclusão](#conclusao)

---

## 1. Introdução à Fluent API <a name="introducao"></a>

A Fluent API é uma abordagem utilizada no Entity Framework Core para configurar o mapeamento entre as classes de entidade e o banco de dados. Ao invés de usar atributos nas classes de entidade, a Fluent API permite configurar as entidades no `DbContext`, tornando o código mais organizado e separado das regras de mapeamento.

---

## 2. Por que usar Fluent API? <a name="porque-usar"></a>

A Fluent API é usada principalmente por sua flexibilidade e controle total sobre como as entidades são mapeadas para o banco de dados. Ela permite:
- **Configurações Avançadas:** Definir relacionamentos complexos, chaves compostas, propriedades computadas, e muito mais.
- **Separação de Código:** Mantém o código de configuração separado das entidades, o que melhora a organização.
- **Sobreposição de Atributos:** Permite sobrescrever as configurações definidas por Data Annotations.
- **Configurações Dinâmicas:** Facilita a aplicação de configurações condicionais.

---

## 3. Configuração da Fluent API <a name="configuracao"></a>

Para configurar a Fluent API, você utiliza o método `OnModelCreating` dentro do seu `DbContext`. Veja um exemplo básico de como iniciar a configuração:

```csharp
public class MeuDbContext : DbContext
{
    public DbSet<Aluno> Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações da Fluent API aqui
        modelBuilder.Entity<Aluno>()
            .HasKey(a => a.Id);
    }
}
```

Nesse exemplo, o método `OnModelCreating` é usado para configurar a entidade `Aluno` e definir que `Id` é a chave primária.

---

## 4. Principais Métodos e Configurações <a name="principais-metodos"></a>

### 4.1 Configuração de Propriedades <a name="configuracao-propriedades"></a>

Configura propriedades específicas das entidades, como nomes de colunas, tipos de dados, e valores padrões.

```csharp
modelBuilder.Entity<Aluno>()
    .Property(a => a.Nome)
    .HasColumnName("NomeCompleto")
    .HasMaxLength(50)
    .IsRequired();
```

- **`HasColumnName`:** Define o nome da coluna no banco de dados.
- **`HasMaxLength`:** Define o tamanho máximo do campo.
- **`IsRequired`:** Indica que o campo é obrigatório.

### 4.2 Configuração de Tipos e Tamanhos <a name="configuracao-tipos"></a>

Permite definir tipos específicos para colunas.

```csharp
modelBuilder.Entity<Aluno>()
    .Property(a => a.DataNascimento)
    .HasColumnType("datetime");
```

- **`HasColumnType`:** Define o tipo de dados da coluna no banco de dados.

### 4.3 Configuração de Chaves Primárias <a name="configuracao-chaves"></a>

Configura as chaves primárias das tabelas.

```csharp
modelBuilder.Entity<Aluno>()
    .HasKey(a => a.Id);
```

- **`HasKey`:** Define a chave primária de uma entidade.

### 4.4 Configuração de Relacionamentos <a name="configuracao-relacionamentos"></a>

Define as relações entre entidades, como um-para-um, um-para-muitos, e muitos-para-muitos.

```csharp
modelBuilder.Entity<Aluno>()
    .HasMany(a => a.Disciplinas)
    .WithOne(d => d.Aluno)
    .HasForeignKey(d => d.AlunoId);
```

- **`HasMany` e `WithOne`:** Configura o relacionamento um-para-muitos.
- **`HasForeignKey`:** Define a chave estrangeira.

### 4.5 Configuração de Índices <a name="configuracao-indices"></a>

Cria índices para melhorar a performance das consultas.

```csharp
modelBuilder.Entity<Aluno>()
    .HasIndex(a => a.Nome)
    .HasDatabaseName("Idx_Aluno_Nome");
```

- **`HasIndex`:** Cria um índice.
- **`HasDatabaseName`:** Define o nome do índice no banco de dados.

### 4.6 Configuração de Campos Computados <a name="configuracao-campos"></a>

Define campos calculados pelo banco de dados.

```csharp
modelBuilder.Entity<Aluno>()
    .Property(a => a.Idade)
    .HasComputedColumnSql("DATEDIFF(YEAR, DataNascimento, GETDATE())");
```

- **`HasComputedColumnSql`:** Configura uma coluna como computada no SQL.

### 4.7 Configuração de Restrições e Regras <a name="configuracao-regras"></a>

Define regras de validação e restrições de banco.

```csharp
modelBuilder.Entity<Aluno>()
    .HasCheckConstraint("CK_Aluno_Idade", "Idade >= 18");
```

- **`HasCheckConstraint`:** Define uma restrição de validação para os dados da tabela.

---

## 5. Exemplos Práticos <a name="exemplos"></a>

### Exemplo Completo de Configuração

```csharp
public class MeuDbContext : DbContext
{
    public DbSet<Aluno> Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.DataNascimento)
                .HasColumnType("datetime");

            entity.HasMany(e => e.Disciplinas)
                .WithOne(d => d.Aluno)
                .HasForeignKey(d => d.AlunoId);

            entity.HasIndex(e => e.Nome)
                .HasDatabaseName("Idx_Aluno_Nome");

            entity.Property(e => e.Idade)
                .HasComputedColumnSql("DATEDIFF(YEAR, DataNascimento, GETDATE())");

            entity.HasCheckConstraint("CK_Aluno_Idade", "Idade >= 18");
        });
    }
}
```

---

## 6. Conclusão <a name="conclusao"></a>

A Fluent API é uma ferramenta poderosa no Entity Framework Core que oferece controle total sobre o mapeamento de entidades para o banco de dados. Com ela, você pode definir propriedades, relacionamentos, restrições, e muito mais de maneira clara e estruturada. Usar Fluent API não só melhora a organização do código como também facilita a manutenção e evolução do projeto.

