## **Lista Detalhada de Data Annotations no ASP.NET Core 8.0 para DTOs**

As Data Annotations no ASP.NET Core 8.0 fornecem uma maneira poderosa de adicionar metadados a propriedades de classes, o que pode ser usado para validação, formatação e outras funcionalidades em seus DTOs (Data Transfer Objects).

**Lista Completa:**

| Data Annotation | Descrição | Exemplo |
|---|---|---|
| **[Required]** | Indica que a propriedade é obrigatória e não pode ser nula. | `[Required]` |
| **[StringLength(n)]** | Define o tamanho máximo da string que pode ser armazenada na propriedade. | `[StringLength(50)]` |
| **[Range(min, max)]** | Define um intervalo válido de valores para a propriedade. | `[Range(18, 120)]` |
| **[RegularExpression(pattern)]** | Define uma expressão regular que a propriedade deve seguir. | `[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]` |
| **[Min(minValue)]** | Define o valor mínimo permitido para a propriedade. | `[Min(10)]` |
| **[Max(maxValue)]** | Define o valor máximo permitido para a propriedade. | `[Max(500)]` |
| **[DataType(DataTypeName)]** | Define o tipo de dados da propriedade. | `[DataType(DataType.EmailAddress)]` |
| **[Display(Name = "Nome Completo")]** | Define o nome de exibição da propriedade para validação e outras funcionalidades. | `[Display(Name = "Nome Completo")]` |
| **[EmailAddress]** | Valida se a propriedade é um endereço de email válido. | `[EmailAddress]` |
| **[Phone]** | Valida se a propriedade é um número de telefone válido. | `[Phone]` |
| **[Url]** | Valida se a propriedade é um URL válido. | `[Url]` |
| **[CreditCard]** | Valida se a propriedade é um número de cartão de crédito válido. | `[CreditCard]` |
| **[Compare(otherProperty)]** | Compara o valor da propriedade com o valor de outra propriedade. | `[Compare("SenhaConfirmacao")]` |
| **[Remote(action, controller)]** | Valida o valor da propriedade usando uma ação remota em outro controller. | `[Remote("VerificarEmailDisponivel", "Usuario")]` |
| **[ValidateIf(predicate, ErrorMessage)]** | Valida a propriedade apenas se a condição especificada for verdadeira. | `[ValidateIf("IsAdmin", ErrorMessage = "Somente administradores podem editar este campo")]` |
| **[CustomValidation(validatorType, ErrorMessage)]** | Usa um validador personalizado para validar a propriedade. | `[CustomValidation(typeof(ValidadorDeCPF), ErrorMessage = "CPF inválido")]` |

**Observações:**

* As Data Annotations podem ser aplicadas a propriedades de qualquer tipo em seus DTOs.
* Você pode combinar várias Data Annotations em uma única propriedade.
* As Data Annotations podem ser usadas em conjunto com outras técnicas de validação, como validação do lado do servidor.
* É importante usar Data Annotations de forma adequada para garantir a qualidade e confiabilidade dos dados em seus DTOs.

**Recursos Adicionais:**

* Documentação oficial do ASP.NET Core sobre Data Annotations: [https://www.ingles.com/traductor/remove%20invalid](https://www.ingles.com/traductor/remove%20invalid)
* Tutoriais sobre Data Annotations:
    * [https://www.youtube.com/watch?v=n1JrknztoY0](https://www.youtube.com/watch?v=n1JrknztoY0)
    * [https://www.youtube.com/watch?v=3LgG4aZPxJI](https://www.youtube.com/watch?v=3LgG4aZPxJI)

**Espero que esta lista detalhada e os recursos adicionais sejam úteis para você!**
