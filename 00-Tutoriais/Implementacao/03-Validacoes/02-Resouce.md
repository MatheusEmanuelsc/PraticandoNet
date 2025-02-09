# Localização e Uso de Resources em Exceções no .NET 8

## Índice
1. [Introdução](#introducao)
2. [Criando um Resource (.resx)](#criando-resource)
3. [Alterando o Modificador de Acesso](#alterando-modificador)
4. [Usando Resources em Validações](#usando-resources)
5. [Localização da API](#localizando-api)
6. [Criando Middleware para Definição de Idioma](#criando-middleware)
7. [Testando com Postman](#testando-postman)

---

## 1. Introdução {#introducao}

A localização (ou internacionalização) é essencial para tornar uma aplicação acessível para usuários de diferentes idiomas. No .NET, podemos usar arquivos `Resource (.resx)` para armazenar mensagens padronizadas de erro ou feedback ao usuário. Esse método permite que as mensagens sejam alteradas sem modificar o código-fonte da aplicação.

---

## 2. Criando um Resource (.resx) {#criando-resource}

No Visual Studio:
1. Clique com o botão direito no projeto.
2. Selecione **Add > New Item**.
3. Escolha **Resource File (.resx)** e nomeie como `ResourceErrorMessages.resx`.
4. No editor, adicione chaves e valores para os erros da aplicação:

| **Key (Chave)**          | **Value (Valor)**                  |  
|--------------------------|------------------------------------|  
| NAME_IS_REQUIRED         | The name is required              |  
| EMAIL_IS_REQUIRED        | The email is required             |  
| PHONE_NUMBER_IS_REQUIRED | The phone number is required      |  

No Rider:
1. Abra o arquivo `.resx` no **Solution Explorer**.
2. Clique com o botão direito e selecione **Properties**.
3. Altere a propriedade **Custom Tool** para `PublicResXFileCodeGenerator`.
4. Se houver **Custom Tool Namespace**, deixe em branco ou defina o namespace desejado.
5. Salve e compile o projeto.

---

## 3. Alterando o Modificador de Acesso {#alterando-modificador}

Por padrão, os arquivos `.resx` geram classes `internal`, o que impede seu uso em outras partes do projeto. Para alterar isso:

- **Visual Studio**: No editor de recursos, altere a opção de `internal` para `public`.
- **Rider**: Use o método descrito acima.

Agora, o arquivo `.Designer.cs` gerado conterá classes `public`.

---

## 4. Usando Resources em Validações {#usando-resources}

Os arquivos `.resx` podem ser usados em validações, como no FluentValidation:

```csharp
public class RegisterCustomerValidator : AbstractValidator<Customer>
{
    public RegisterCustomerValidator()
    {
        RuleFor(customer => customer.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.NAME_IS_REQUIRED);
        
        RuleFor(customer => customer.Email)
            .NotEmpty().EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_IS_REQUIRED);
        
        RuleFor(customer => customer.PhoneNumber)
            .NotEmpty().WithMessage(ResourceErrorMessages.PHONE_NUMBER_IS_REQUIRED);
    }
}
```

Isso permite manter mensagens centralizadas e facilmente traduzíveis.

---

## 5. Localização da API {#localizando-api}

Para suportar diferentes idiomas, crie arquivos `.resx` adicionais:

- `ResourceErrorMessages.pt-BR.resx`
- `ResourceErrorMessages.es-ES.resx`

Cada arquivo conterá as mesmas **chaves**, mas os valores traduzidos:

| **Chave**                 | **Inglês**                         | **Espanhol**                                | **Português**                                   |
|---------------------------|------------------------------------|--------------------------------------------|------------------------------------------------|
| EMAIL_IS_REQUIRED         | The email is required             | El correo electrónico es obligatorio.       | O e-mail é obrigatório.                        |
| NAME_IS_REQUIRED          | The name is required              | El nombre es obligatorio.                  | O nome é obrigatório.                         |
| PHONE_NUMBER_IS_REQUIRED  | The phone number is required      | El número de teléfono es obligatorio.      | O número de telefone é obrigatório.           |
| UNKNOWN_ERROR             | Unknown error                     | Error desconocido.                         | Erro desconhecido.                            |


---

## 6. Criando Middleware para Definição de Idioma {#criando-middleware}

Para definir o idioma com base no cabeçalho `Accept-Language`, crie a classe `CultureMiddleware`:

```csharp
using System.Globalization;

namespace CashBank.Api.Middleware;

public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var supportedLanguages = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList();
        var requestCulture = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();

        var cultureInfo = new CultureInfo("en"); // Padrão: inglês
        
        if (!string.IsNullOrWhiteSpace(requestCulture) && supportedLanguages.Exists(lang => lang.Name.Equals(requestCulture)))
        {
            cultureInfo = new CultureInfo(requestCulture);
        }

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(httpContext);
    }
}
```

Agora, registre o middleware no `Program.cs`:

```csharp
app.UseMiddleware<CultureMiddleware>();
```

Isso permite que a API responda no idioma correto, de acordo com o cabeçalho `Accept-Language`.

---

## 7. Testando com Postman {#testando-postman}

O Swagger pode não funcionar corretamente para testes de localização, então use o **Postman**:

1. Escolha uma requisição (GET, POST, etc.).
2. Na aba **Headers**, adicione:
   - **Key:** `Accept-Language`
   - **Value:** `pt-BR` (ou outro idioma desejado)
3. Envie a requisição e verifique a resposta no idioma esperado.

Agora sua API está configurada para localização!

---

### Conclusão

Com esse tutorial, aprendemos a criar e configurar **Resources (.resx)**, modificar seu acesso, utilizá-los em validações, localizar mensagens da API e definir idiomas dinamicamente através de middleware. Isso garante uma aplicação mais acessível e adaptável para diferentes públicos.

