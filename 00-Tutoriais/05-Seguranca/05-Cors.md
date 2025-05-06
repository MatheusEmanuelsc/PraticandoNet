# Tutorial Passo a Passo: Implementando CORS no .NET Moderno

## Índice
1. [Introdução ao CORS](#introdução-ao-cors)
2. [O que é CORS e por que é necessário](#o-que-é-cors-e-por-que-é-necessário)
3. [Implementação do CORS no .NET 6+](#implementação-do-cors-no-net-6)
   - [Instalação dos pacotes necessários](#instalação-dos-pacotes-necessários)
   - [Configuração básica no Program.cs](#configuração-básica-no-programcs)
   - [Aplicando CORS no pipeline da aplicação](#aplicando-cors-no-pipeline-da-aplicação)
4. [Configurações avançadas de CORS](#configurações-avançadas-de-cors)
   - [Criando políticas CORS personalizadas](#criando-políticas-cors-personalizadas)
   - [Configurando diferentes políticas para diferentes endpoints](#configurando-diferentes-políticas-para-diferentes-endpoints)
5. [Configuração CORS em controladores específicos](#configuração-cors-em-controladores-específicos)
   - [Usando atributos CORS em controladores e ações](#usando-atributos-cors-em-controladores-e-ações)
6. [CORS para Minimal APIs](#cors-para-minimal-apis)
7. [Testando a configuração CORS](#testando-a-configuração-cors)
   - [Ferramentas para testar CORS](#ferramentas-para-testar-cors)
   - [Sinais de que CORS está funcionando corretamente](#sinais-de-que-cors-está-funcionando-corretamente)
8. [Solução de problemas comuns](#solução-de-problemas-comuns)
9. [Melhores práticas de segurança](#melhores-práticas-de-segurança)

## Introdução ao CORS

Cross-Origin Resource Sharing (CORS) é um mecanismo essencial de segurança web que permite que aplicações em um domínio acessem recursos de outro domínio de forma controlada e segura. Este tutorial explica como implementar CORS em aplicações .NET modernas (a partir do .NET 6), onde o arquivo de configuração Startup.cs foi substituído pelo Program.cs.

## O que é CORS e por que é necessário

### Definição
CORS (Cross-Origin Resource Sharing) é um mecanismo que permite que recursos restritos em uma página web sejam solicitados a partir de outro domínio fora do domínio de onde o recurso se originou.

### Por que CORS é necessário?
Os navegadores modernos implementam a "política de mesma origem" (same-origin policy) como medida de segurança. Esta política impede que o JavaScript de um site faça requisições para um domínio diferente do que o serviu. CORS é a solução padronizada para permitir essas requisições cross-origin de forma controlada.

### Cenários comuns para CORS:
- Frontend em um domínio (exemplo.com) consumindo APIs de outro domínio (api.exemplo.com)
- Aplicações SPA (Single Page Applications) que chamam APIs de back-end hospedadas separadamente
- Serviços de microsserviços distribuídos que precisam se comunicar entre si

## Implementação do CORS no .NET 6+

### Instalação dos pacotes necessários

O suporte a CORS já está incluído no pacote `Microsoft.AspNetCore.App`, que é referenciado por padrão nos projetos ASP.NET Core. Normalmente, você não precisa instalar pacotes adicionais, mas se necessário:

```bash
dotnet add package Microsoft.AspNetCore.Cors
```

### Configuração básica no Program.cs

No .NET 6+ (sem a classe Startup), a configuração de CORS é feita diretamente no arquivo Program.cs:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao container
builder.Services.AddControllers();

// Configuração básica de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://exemplo.com", "https://www.exemplo.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurar o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Habilitar CORS no pipeline
app.UseCors();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Aplicando CORS no pipeline da aplicação

A ordem das chamadas de middleware é importante no ASP.NET Core:

```csharp
// Ordem recomendada no pipeline
app.UseRouting();
app.UseCors(); // CORS deve vir antes de Authorization, mas depois de Routing
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```

## Configurações avançadas de CORS

### Criando políticas CORS personalizadas

Você pode definir múltiplas políticas CORS e aplicá-las conforme necessário:

```csharp
builder.Services.AddCors(options =>
{
    // Política permissiva para desenvolvimento
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    // Política restritiva para produção
    options.AddPolicy("ProdPolicy", policy =>
    {
        policy.WithOrigins("https://app.minhaempresa.com", "https://admin.minhaempresa.com")
              .WithHeaders("Content-Type", "Authorization")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .AllowCredentials();
    });
});
```

### Configurando diferentes políticas para diferentes endpoints

Ao usar políticas personalizadas, você precisa especificar qual política usar:

```csharp
// Aplicar uma política específica globalmente
app.UseCors("ProdPolicy");

// OU aplicar diferentes políticas para diferentes grupos de endpoints
app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers()
            .RequireCors("ProdPolicy");
    
    endpoints.MapControllerRoute(
        name: "api",
        pattern: "api/{controller}/{action}")
            .RequireCors("DevPolicy");
});
```

## Configuração CORS em controladores específicos

### Usando atributos CORS em controladores e ações

Você pode aplicar políticas CORS específicas a controladores ou ações individuais:

```csharp
[EnableCors("DevPolicy")]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("CORS habilitado para esta ação com DevPolicy");
    }
    
    [HttpPost]
    [EnableCors("ProdPolicy")] // Sobrescreve a política do controlador
    public IActionResult Post()
    {
        return Ok("CORS habilitado para esta ação com ProdPolicy");
    }
    
    [HttpDelete]
    [DisableCors] // Desabilita CORS para esta ação
    public IActionResult Delete()
    {
        return Ok("CORS desabilitado para esta ação");
    }
}
```

## CORS para Minimal APIs

No .NET 6+ com Minimal APIs, você pode aplicar CORS assim:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins("https://exemplo.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Aplicar CORS globalmente
app.UseCors();

// OU aplicar política específica a endpoints específicos
app.MapGet("/api/dados", () => 
{
    return new[] { "valor1", "valor2" };
})
.RequireCors("AllowSpecificOrigin");

app.Run();
```

## Testando a configuração CORS

### Ferramentas para testar CORS
1. **DevTools do navegador**: Observe as requisições na aba Network
2. **Postman**: Para testar requisições pré-flight OPTIONS
3. **CORS Test**: Sites como https://cors-test.codehappy.dev/

### Sinais de que CORS está funcionando corretamente
- Ausência de erros CORS no console do navegador
- Respostas bem-sucedidas para requisições entre origens diferentes
- Cabeçalhos de resposta CORS corretos (`Access-Control-Allow-Origin`, etc.)

Para testar manualmente com JavaScript:

```javascript
fetch('https://minha-api.com/dados', {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
})
.then(response => response.json())
.then(data => console.log('Sucesso:', data))
.catch(error => console.error('Erro:', error));
```

## Solução de problemas comuns

1. **Erro "No 'Access-Control-Allow-Origin' header is present"**:
   - Verifique se o domínio origem está corretamente incluído nas origens permitidas
   - Confirme se `app.UseCors()` está na ordem correta do pipeline

2. **Problemas com credenciais**:
   - Para requisições com cookies/credentials, use `AllowCredentials()` e especifique as origens exatas (não use `AllowAnyOrigin()`)

3. **Erro em requisições preflight OPTIONS**:
   - Verifique se os métodos HTTP e cabeçalhos personalizados estão incluídos nas configurações

4. **CORS não funciona em produção mas funciona em desenvolvimento**:
   - Confirme que as URLs de produção estão na lista de origens permitidas
   - Verifique se não há proxies/firewalls bloqueando cabeçalhos CORS

## Melhores práticas de segurança

1. **Evite** `AllowAnyOrigin()`, `AllowAnyHeader()`, e `AllowAnyMethod()` em produção
2. **Especifique** exatamente quais origens, cabeçalhos e métodos são permitidos
3. **Use** `WithOrigins()` com URLs completas, incluindo o protocolo (https://)
4. **Considere** o uso de diferentes políticas CORS para diferentes partes da sua API
5. **Cuidado** ao habilitar `AllowCredentials()` - isso requer origens específicas
6. **Não** exponha APIs sensíveis a origens não confiáveis
7. **Documente** suas políticas CORS como parte da documentação da API
8. **Desenvolva** usando configurações CORS baseadas em ambiente (dev vs prod)

Estas orientações ajudarão você a implementar CORS de forma segura em sua aplicação .NET moderna, garantindo que você tenha controle total sobre quais recursos podem ser acessados por aplicações em diferentes domínios.