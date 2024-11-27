

---

# Habilitando CORS em uma Web API .NET: Tutorial Passo a Passo

## 1. M�todo 1: Habilitando CORS com Middleware

### Definindo Pol�ticas de CORS

Para habilitar CORS usando middleware, primeiro � necess�rio definir as pol�ticas de CORS no arquivo `Program.cs` durante a configura��o dos servi�os:

```csharp
var OrigensComAcessoPermitido = "_origensComAcessoPermitido";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: OrigensComAcessoPermitido, policy =>
    {
        policy.WithOrigins("http://www.apirequest.io") // Exemplo de libera��o via CORS
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Configurando o Middleware

Depois de definir a pol�tica, � necess�rio configurar o middleware CORS na pipeline de requisi��es:

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(OrigensComAcessoPermitido); // Adiciona o middleware CORS
app.UseAuthorization();
```

**Importante:** O middleware CORS deve ser adicionado **antes** do middleware de autoriza��o (`app.UseAuthorization();`), mas **depois** de `app.UseRouting();`.

---

## 2. M�todo 2: Habilitando CORS com Atributos

### Aplicando Atributos nos Controladores

Para aplicar uma pol�tica CORS espec�fica a um controlador inteiro, utilize o atributo `[EnableCors]`:

```csharp
[EnableCors("_origensComAcessoPermitido")]
[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    // M�todos do controlador
}
```

### Aplicando Atributos em M�todos de A��o

Para aplicar CORS a um m�todo de a��o espec�fico, utilize:

```csharp
[EnableCors("_origensComAcessoPermitido")]
[HttpGet]
public IActionResult Get()
{
    // C�digo do m�todo
}
```

### Desabilitando CORS em Endpoints Espec�ficos

Para desabilitar CORS em um endpoint espec�fico, use o atributo `[DisableCors]`:

```csharp
[DisableCors]
[HttpPost]
public IActionResult Post()
{
    // C�digo do m�todo
}
```

---

## 3. M�todo 3: Habilitando CORS Globalmente

Para habilitar CORS globalmente para toda a aplica��o, sem a necessidade de definir atributos nos controladores ou m�todos:

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

Para permitir o envio de credenciais em requisi��es de origem cruzada, configure a pol�tica CORS para permitir credenciais:

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

**Aten��o:** Permitir o envio de credenciais entre origens cruzadas pode representar um risco de seguran�a.

