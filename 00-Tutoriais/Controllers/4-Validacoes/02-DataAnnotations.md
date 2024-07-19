```markdown
# Data Annotations no ASP.NET Core

As Data Annotations no ASP.NET Core são um conjunto de atributos que permitem adicionar validações e metadados a modelos de dados. Esses atributos ajudam a definir regras de validação, restrições de formato e fornecer informações adicionais sobre os dados, que podem ser utilizadas por frameworks como o Entity Framework e a ASP.NET Core MVC para validação automática de entradas e geração de interfaces de usuário.

## Índice

1. [Required](#required)
2. [StringLength](#stringlength)
3. [MaxLength](#maxlength)
4. [MinLength](#minlength)
5. [Range](#range)
6. [RegularExpression](#regularexpression)
7. [EmailAddress](#emailaddress)
8. [Phone](#phone)
9. [Url](#url)
10. [CreditCard](#creditcard)
11. [Compare](#compare)
12. [Custom Validation](#custom-validation)
13. [Display](#display)
14. [ScaffoldColumn](#scaffoldcolumn)
15. [ConcurrencyCheck](#concurrencycheck)
16. [Timestamp](#timestamp)
17. [Key](#key)
18. [DatabaseGenerated](#databasegenerated)
19. [ForeignKey](#foreignkey)
20. [InverseProperty](#inverseproperty)

## Required
O atributo `Required` garante que uma propriedade não seja nula ou vazia.

### Exemplo:
```csharp
public class Produto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; }
}
```

## StringLength
O atributo `StringLength` especifica o tamanho máximo e/ou mínimo de uma cadeia de caracteres.

### Exemplo:
```csharp
public class Produto
{
    [StringLength(100, MinimumLength = 10, ErrorMessage = "O nome deve ter entre 10 e 100 caracteres")]
    public string Nome { get; set; }
}
```

## MaxLength
O atributo `MaxLength` define o comprimento máximo de uma cadeia de caracteres ou uma matriz.

### Exemplo:
```csharp
public class Produto
{
    [MaxLength(50, ErrorMessage = "O nome pode ter no máximo 50 caracteres")]
    public string Nome { get; set; }
}
```

## MinLength
O atributo `MinLength` define o comprimento mínimo de uma cadeia de caracteres ou uma matriz.

### Exemplo:
```csharp
public class Produto
{
    [MinLength(5, ErrorMessage = "O nome deve ter no mínimo 5 caracteres")]
    public string Nome { get; set; }
}
```

## Range
O atributo `Range` especifica um intervalo válido para valores numéricos.

### Exemplo:
```csharp
public class Produto
{
    [Range(1, 100, ErrorMessage = "O preço deve estar entre 1 e 100")]
    public decimal Preco { get; set; }
}
```

## RegularExpression
O atributo `RegularExpression` valida a entrada de acordo com uma expressão regular.

### Exemplo:
```csharp
public class Usuario
{
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "O nome de usuário só pode conter letras e números")]
    public string NomeUsuario { get; set; }
}
```

## EmailAddress
O atributo `EmailAddress` valida se a entrada é um endereço de e-mail.

### Exemplo:
```csharp
public class Usuario
{
    [EmailAddress(ErrorMessage = "O e-mail não é válido")]
    public string Email { get; set; }
}
```

## Phone
O atributo `Phone` valida se a entrada é um número de telefone.

### Exemplo:
```csharp
public class Usuario
{
    [Phone(ErrorMessage = "O número de telefone não é válido")]
    public string Telefone { get; set; }
}
```

## Url
O atributo `Url` valida se a entrada é uma URL.

### Exemplo:
```csharp
public class Website
{
    [Url(ErrorMessage = "A URL não é válida")]
    public string Endereco { get; set; }
}
```

## CreditCard
O atributo `CreditCard` valida se a entrada é um número de cartão de crédito.

### Exemplo:
```csharp
public class Pagamento
{
    [CreditCard(ErrorMessage = "O número do cartão de crédito não é válido")]
    public string NumeroCartao { get; set; }
}
```

## Compare
O atributo `Compare` compara o valor de duas propriedades.

### Exemplo:
```csharp
public class Usuario
{
    public string Senha { get; set; }

    [Compare("Senha", ErrorMessage = "As senhas não correspondem")]
    public string ConfirmacaoSenha { get; set; }
}
```

## Custom Validation
Você pode criar validações personalizadas usando atributos customizados.

### Exemplo:
```csharp
public class Produto
{
    [CustomValidation(typeof(MinhaValidacao), "ValidaNome")]
    public string Nome { get; set; }
}

public class MinhaValidacao
{
    public static ValidationResult ValidaNome(string nome, ValidationContext context)
    {
        if (nome == "Invalido")
        {
            return new ValidationResult("O nome não pode ser 'Invalido'");
        }
        return ValidationResult.Success;
    }
}
```

## Display
O atributo `Display` especifica detalhes sobre como uma propriedade deve ser exibida.

### Exemplo:
```csharp
public class Produto
{
    [Display(Name = "Nome do Produto", Description = "Nome completo do produto")]
    public string Nome { get; set; }
}
```

## ScaffoldColumn
O atributo `ScaffoldColumn` define se uma propriedade deve ser gerada ou não por scaffolding.

### Exemplo:
```csharp
public class Produto
{
    [ScaffoldColumn(false)]
    public string NomeInterno { get; set; }
}
```

## ConcurrencyCheck
O atributo `ConcurrencyCheck` marca uma propriedade para verificação de concorrência.

### Exemplo:
```csharp
public class Produto
{
    [ConcurrencyCheck]
    public int QuantidadeEmEstoque { get; set; }
}
```

## Timestamp
O atributo `Timestamp` indica que uma coluna de banco de dados deve ser usada como um carimbo de tempo para controle de concorrência.

### Exemplo:
```csharp
public class Produto
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

## Key
O atributo `Key` especifica que uma propriedade é a chave primária de uma entidade.

### Exemplo:
```csharp
public class Produto
{
    [Key]
    public int ProdutoId { get; set; }
}
```

## DatabaseGenerated
O atributo `DatabaseGenerated` especifica como o valor de uma propriedade é gerado pelo banco de dados.

### Exemplo:
```csharp
public class Produto
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProdutoId { get; set; }
}
```

## ForeignKey
O atributo `ForeignKey` define uma propriedade como chave estrangeira.

### Exemplo:
```csharp
public class Pedido
{
    public int ClienteId { get; set; }

    [ForeignKey("ClienteId")]
    public Cliente Cliente { get; set; }
}
```

## InverseProperty
O atributo `InverseProperty` especifica a propriedade inversa em uma relação.

### Exemplo:
```csharp
public class Pedido
{
    public int ClienteId { get; set; }

    [InverseProperty("Pedidos")]
    public Cliente Cliente { get; set; }
}

public class Cliente
{
    public int ClienteId { get; set; }

    [InverseProperty("Cliente")]
    public ICollection<Pedido> Pedidos { get; set; }
}
```

Este resumo completo das Data Annotations no ASP.NET Core cobre os atributos mais utilizados para validação e configuração de modelos de dados. Cada atributo foi descrito com um exemplo para facilitar a compreensão de sua utilização prática. 
```