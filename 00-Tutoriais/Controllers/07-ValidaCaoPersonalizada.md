```markdown
## Custom Validation

Além das validações padrão fornecidas pelas Data Annotations, é possível criar validações personalizadas para atender a requisitos específicos do seu aplicativo. Isso é útil quando as validações padrão não são suficientes para validar os dados de acordo com as regras de negócio ou quando você precisa de uma lógica mais complexa para validar uma propriedade.

### Exemplo:

Suponha que você tenha uma classe `Produto` e precise validar o nome do produto de acordo com regras específicas do seu domínio. Aqui está um exemplo de como implementar uma validação personalizada para o nome do produto:

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

Neste exemplo, criamos uma classe `MinhaValidacao` com um método estático `ValidaNome`, que recebe o valor da propriedade sendo validada e um contexto de validação. Dentro deste método, realizamos a lógica de validação personalizada, retornando `ValidationResult.Success` se a validação for bem-sucedida ou uma instância de `ValidationResult` com uma mensagem de erro caso contrário.

Em seguida, aplicamos essa validação personalizada à propriedade `Nome` da classe `Produto` usando o atributo `CustomValidation`. Este atributo recebe como parâmetros o tipo da classe de validação personalizada (`MinhaValidacao`) e o nome do método de validação (`ValidaNome`).

Ao usar validações personalizadas, você pode estender facilmente a funcionalidade de validação do ASP.NET Core para atender às necessidades específicas do seu aplicativo.
```