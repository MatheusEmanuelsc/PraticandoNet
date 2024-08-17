

---

# Habilitando CORS em uma Web API .NET: Tutorial Passo a Passo

## 1. Método 1: Habilitando CORS com Middleware

### Definindo Políticas de CORS

Para habilitar CORS usando middleware, primeiro é necessário definir as políticas de CORS no arquivo `Program.cs` durante a configuração dos serviços:

```csharp
var OrigensComAcessoPermitido = "_origensComAcessoPermitido";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: OrigensComAcessoPermitido, policy =>
    {
        policy.WithOrigins("http://www.apirequest.io") // Exemplo de liberação via CORS
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Configurando o Middleware

Depois de definir a política, é necessário configurar o middleware CORS na pipeline de requisições:

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(OrigensComAcessoPermitido); // Adiciona o middleware CORS
app.UseAuthorization();
```

**Importante:** O middleware CORS deve ser adicionado **antes** do middleware de autorização (`app.UseAuthorization();`), mas **depois** de `app.UseRouting();`.

---

## 2. Método 2: Habilitando CORS com Atributos

### Aplicando Atributos nos Controladores

Para aplicar uma política CORS específica a um controlador inteiro, utilize o atributo `[EnableCors]`:

```csharp
[EnableCors("_origensComAcessoPermitido")]
[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    // Métodos do controlador
}
```

### Aplicando Atributos em Métodos de Ação

Para aplicar CORS a um método de ação específico, utilize:

```csharp
[EnableCors("_origensComAcessoPermitido")]
[HttpGet]
public IActionResult Get()
{
    // Código do método
}
```

### Desabilitando CORS em Endpoints Específicos

Para desabilitar CORS em um endpoint específico, use o atributo `[DisableCors]`:

```csharp
[DisableCors]
[HttpPost]
public IActionResult Post()
{
    // Código do método
}
```

---

## 3. Método 3: Habilitando CORS Globalmente

Para habilitar CORS globalmente para toda a aplicação, sem a necessidade de definir atributos nos controladores ou métodos:

```csharp
var OrigensComAcessoPermitido = "_origensComAcessoPermitido";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://www.apirequest.io")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors(); // Adiciona o middleware CORS globalmente
```

---

## 4. Envio de Credenciais com CORS

### Configurando o Servidor

Para permitir o envio de credenciais em requisições de origem cruzada, configure a política CORS para permitir credenciais:

```csharp
policy.WithOrigins("http://www.apirequest.io")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
```

### Configurando o Cliente

No lado do cliente, defina `XMLHttpRequest.withCredentials` como `true` para enviar credenciais:

```javascript
var xhr = new XMLHttpRequest();
xhr.open('GET', 'https://sua-api-com-seu-endpoint', true);
xhr.withCredentials = true;
xhr.send(null);
```

**Atenção:** Permitir o envio de credenciais entre origens cruzadas pode representar um risco de segurança.

