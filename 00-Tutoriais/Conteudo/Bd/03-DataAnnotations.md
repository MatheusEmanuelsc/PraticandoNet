## Índice

1. [Introdução às Anotações de Dados](#introducao)
2. [Anotações de Dados Padrão](#anotacoes-padrao)
   - [Required](#required)
   - [EmailAddress](#emailaddress)
   - [RegularExpression](#regularexpression)
   - [MinLength](#minlength)
   - [MaxLength](#maxlength)
   - [Range](#range)
   - [StringLength](#stringlength)
   - [Compare](#compare)
   - [CreditCard](#creditcard)
   - [Phone](#phone)
   - [Url](#url)
3. [Anotações de Dados Avançadas no .NET 8](#anotacoes-avancadas)
   - [AllowedValues](#allowedvalues)
   - [DeniedValues](#deniedvalues)
   - [Base64String](#base64string)
   - [Length](#length)
   - [Range com Exclusividade](#range-exclusivo)
4. [Exemplos de Uso](#exemplos-uso)
5. [Anotações de Dados para Sobrescrever Convenções do EF](#anotacoes-ef)
   - [Key](#key)
   - [ForeignKey](#foreignkey)
   - [Column](#column)
   - [Required (EF)](#required-ef)
   - [MaxLength (EF)](#maxlength-ef)
   - [StringLength (EF)](#stringlength-ef)
   - [ConcurrencyCheck](#concurrencycheck)
   - [Timestamp](#timestamp)
   - [DatabaseGenerated](#databasegenerated)
   - [NotMapped](#notmapped)
   - [InverseProperty](#inverseproperty)

---

## Anotações de Dados Padrão <a name="anotacoes-padrao"></a>

### Required <a name="required"></a>
Garante que a propriedade tenha um valor atribuído.

### EmailAddress <a name="emailaddress"></a>
Valida o formato de um endereço de e-mail.

### RegularExpression <a name="regularexpression"></a>
Valida o formato de um valor específico usando uma expressão regular.

### MinLength <a name="minlength"></a>
Define o comprimento mínimo de uma string.

### MaxLength <a name="maxlength"></a>
Define o comprimento máximo de uma string.

### Range <a name="range"></a>
Restringe um valor a um intervalo específico.

### StringLength <a name="stringlength"></a>
Define um comprimento máximo para uma string.

### Compare <a name="compare"></a>
Compara o valor de uma propriedade com outra.

### CreditCard <a name="creditcard"></a>
Valida o formato de um número de cartão de crédito.

### Phone <a name="phone"></a>
Valida o formato de um número de telefone.

### Url <a name="url"></a>
Valida o formato de uma URL.

## Anotações de Dados Avançadas no .NET 8 <a name="anotacoes-avancadas"></a>

### AllowedValues <a name="allowedvalues"></a>
Especifica valores permitidos para uma propriedade.

### DeniedValues <a name="deniedvalues"></a>
Especifica valores não permitidos para uma propriedade.

### Base64String <a name="base64string"></a>
Valida se uma string está codificada em Base64.

### Length <a name="length"></a>
Define o comprimento mínimo e máximo de uma lista.

### Range com Exclusividade <a name="range-exclusivo"></a>
Permite definir se o valor mínimo ou máximo é exclusivo, ou seja, se deve ser excluído do intervalo permitido.

## Exemplos de Uso <a name="exemplos-uso"></a>

### Validação Básica

```csharp
public class CustomerModel
{
    [Required, MinLength(3), MaxLength(30)]
    public string FirstName { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Surname { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [RegularExpression("^([0-9]{5})$")]
    public string ZipCode { get; set; }
}
```

### Validação com Atributos Avançados

```csharp
public class MyValidationModel
{
    [AllowedValues("banana", "apple")]
    public string Fruit { get; set; }

    [DeniedValues(16, 17)]
    public int Age { get; set; }

    [Base64String]
    public string Hash { get; set; }

    [Length(1, 3)]
    public List<string> FavouriteFruits { get; set; }

    [Range(10, 19, MinimumIsExclusive = true, MaximumIsExclusive = true)]
    public int AgeExclusive { get; set; }
}
```

## Anotações de Dados para Sobrescrever Convenções do EF <a name="anotacoes-ef"></a>

### Key <a name="key"></a>
Define a chave primária da tabela.

```csharp
public class Entity
{
    [Key]
    public int Id { get; set; }
}
```

### ForeignKey <a name="foreignkey"></a>
Define a chave estrangeira da relação.

```csharp
public class Order
{
    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }

    public int CustomerId { get; set; }
}
```

### Column <a name="column"></a>
Especifica o nome da coluna no banco de dados.

```csharp
public class Product
{
    [Column("ProductName")]
    public string Name { get; set; }
}
```

### Required (EF) <a name="required-ef"></a>
Especifica que a propriedade é obrigatória no banco de dados.

```csharp
public class User
{
    [Required]
    public string Username { get; set; }
}
```

### MaxLength (EF) <a name="maxlength-ef"></a>
Define o comprimento máximo de uma coluna.

```csharp
public class User
{
    [MaxLength(50)]
    public string Username { get; set; }
}
```

### StringLength (EF) <a name="stringlength-ef"></a>
Define o comprimento máximo para uma string, usado tanto na validação quanto na definição do esquema.

```csharp
public class User
{
    [StringLength(50)]
    public string Username { get; set; }
}
```

### ConcurrencyCheck <a name="concurrencycheck"></a>
Marca a propriedade para verificação de concorrência.

```csharp
public class Product
{
    [ConcurrencyCheck]
    public int Stock { get; set; }
}
```

### Timestamp <a name="timestamp"></a>
Usado para implementar controle de concorrência otimista com um campo de carimbo de tempo.

```csharp
public class Entity
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

### DatabaseGenerated <a name="databasegenerated"></a>
Especifica como o valor de uma propriedade é gerado no banco de dados.

```csharp
public class Entity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}
```

### NotMapped <a name="notmapped"></a>
Indica que uma propriedade não é mapeada para uma coluna no banco de dados.

```csharp
public class User
{
    [NotMapped]
    public int Age { get; set; }
}
```

### InverseProperty <a name="inverseproperty"></a>
Especifica a propriedade de navegação inversa para uma relação.

```csharp
public class Order
{
    public int OrderId { get; set; }

    [InverseProperty("Orders")]
    public Customer Customer { get; set; }
}
```