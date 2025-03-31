
---

# Validação no ASP.NET Core

## Índice
1. [O que é Validação?](#o-que-é-validação)
2. [Validação no ASP.NET Core](#validação-no-aspnet-core)
   - [ModelState](#modelstate)
   - [Data Annotations](#data-annotations)
3. [Lista de Data Annotations Nativas](#lista-de-data-annotations-nativas)
4. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Modelo com Validações](#passo-1-criar-o-modelo-com-validações)
   - [Passo 2: Configurar o Endpoint](#passo-2-configurar-o-endpoint)
   - [Passo 3: Criar uma Data Annotation Personalizada](#passo-3-criar-uma-data-annotation-personalizada)
   - [Passo 4: Testar a Validação](#passo-4-testar-a-validação)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que é Validação?

Validação é o processo de verificar se os dados de entrada atendem a regras específicas, como obrigatoriedade, formato ou intervalo, antes de serem processados, garantindo integridade e segurança na aplicação.

---

## Validação no ASP.NET Core

O ASP.NET Core oferece um sistema de validação nativo que integra *Model Binding* com *Data Annotations* e `ModelState`, permitindo regras simples ou personalizadas.

### ModelState
`ModelState` é um objeto preenchido durante o *Model Binding* que indica se os dados são válidos (`IsValid`) e contém erros detalhados para cada campo inválido.

### Data Annotations
São atributos aplicados às propriedades dos modelos para definir regras de validação. O framework as processa automaticamente ao vincular os dados.

---

## Lista de Data Annotations Nativas

Aqui está a lista completa de *Data Annotations* disponíveis no namespace `System.ComponentModel.DataAnnotations`, com um resumo do que cada uma faz:

- **`[Required]`**: Marca um campo como obrigatório. Falha se for nulo ou vazio.
- **`[StringLength(int maximumLength, int minimumLength)]`**: Define o tamanho máximo e opcionalmente mínimo de uma string.
- **`[Range(int minimum, int maximum)]`**: Restringe um valor numérico a um intervalo (ex.: 1 a 100).
- **`[EmailAddress]`**: Verifica se uma string segue o formato de e-mail.
- **`[RegularExpression(string pattern)]`**: Valida uma string com base em uma expressão regular.
- **`[MinLength(int length)]`**: Define o comprimento mínimo de uma string ou coleção.
- **`[MaxLength(int length)]`**: Define o comprimento máximo de uma string ou coleção.
- **`[Compare(string otherProperty)]`**: Compara duas propriedades para garantir que sejam iguais (ex.: senha e confirmação).
- **`[Url]`**: Verifica se uma string é uma URL válida.
- **`[Phone]`**: Valida se uma string segue o formato de número de telefone.
- **`[CreditCard]`**: Verifica se uma string é um número de cartão de crédito válido (usa algoritmo de Luhn).
- **`[FileExtensions(string extensions)]`**: Valida extensões de arquivo (ex.: "jpg,png").
- **`[DataType(DataType tipo)]`**: Define o tipo de dado (ex.: `DataType.Date`), influenciando validação e renderização.
- **`[Key]`**: Marca uma propriedade como chave primária (usada em Entity Framework, mas não em validação direta).
- **`[Timestamp]`**: Indica que a propriedade é um timestamp (usada em cenários de concorrência, não validação).

---

## Tutorial Passo a Passo

### Passo 1: Criar o Modelo com Validações

Crie um modelo com regras de validação nativas.

```csharp
using System.ComponentModel.DataAnnotations;

public class UsuarioModel
{
    [Required(ErrorMessage = "O ID é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "O ID deve ser maior que 0")]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres")]
    public string Nome { get; set; }

    [Range(18, 100, ErrorMessage = "A idade deve estar entre 18 e 100")]
    public int Idade { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; }
}
```

### Passo 2: Configurar o Endpoint

Crie um controlador que verifica o `ModelState`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    [HttpPost]
    public IActionResult Criar([FromBody] UsuarioModel usuario)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return Ok(new { Mensagem = "Usuário válido", Dados = usuario });
    }
}
```

### Passo 3: Criar uma Data Annotation Personalizada

Crie uma validação customizada para verificar se o nome começa com uma letra maiúscula.

1. **Defina o atributo**:

```csharp
using System.ComponentModel.DataAnnotations;

public class CapitalizedAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success; // Deixa [Required] lidar com nulos
        }

        string input = value.ToString();
        if (char.IsUpper(input[0]))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "O valor deve começar com letra maiúscula");
    }
}
```

2. **Aplique ao modelo**:

Atualize o `UsuarioModel` para incluir a nova validação:

```csharp
public class UsuarioModel
{
    [Required(ErrorMessage = "O ID é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "O ID deve ser maior que 0")]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres")]
    [Capitalized(ErrorMessage = "O nome deve começar com maiúscula")]
    public string Nome { get; set; }

    [Range(18, 100, ErrorMessage = "A idade deve estar entre 18 e 100")]
    public int Idade { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; }
}
```

### Passo 4: Testar a Validação

No `Program.cs`, configure os controladores e teste.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

- **Teste Válido**: `POST /api/usuarios` com `{ "id": 1, "nome": "João", "idade": 25, "email": "joao@email.com" }`
  - Resposta: `200 OK` com `{ "mensagem": "Usuário válido", "dados": { "id": 1, "nome": "João", "idade": 25, "email": "joao@email.com" } }`
- **Teste Inválido**: `POST /api/usuarios` com `{ "id": 0, "nome": "joão", "idade": 15, "email": "invalido" }`
  - Resposta: `400 Bad Request` com:
    ```json
    {
        "Id": ["O ID deve ser maior que 0"],
        "Nome": ["O nome deve começar com maiúscula"],
        "Idade": ["A idade deve estar entre 18 e 100"],
        "Email": ["E-mail inválido"]
    }
    ```

---

## Boas Práticas

1. **Mensagens úteis**: Personalize `ErrorMessage` para clareza.
2. **Validação dupla**: Combine validação no servidor com cliente, mas priorize o servidor.
3. **Regras simples em atributos**: Use *Data Annotations* para validações básicas; para lógica complexa, prefira `IValidatableObject` ou serviços.
4. **Respostas detalhadas**: Retorne todos os erros do `ModelState` para ajudar o cliente.
5. **Teste extensivo**: Valide casos como nulos, valores extremos e formatos inválidos.

---

## Conclusão

A validação no ASP.NET Core é robusta e flexível, com *Data Annotations* nativas cobrindo a maioria dos cenários comuns e a possibilidade de criar validações personalizadas para necessidades específicas. Este tutorial demonstra como usar validações prontas, criar uma customizada e integrá-las ao fluxo da aplicação, garantindo dados consistentes com esforço mínimo.

