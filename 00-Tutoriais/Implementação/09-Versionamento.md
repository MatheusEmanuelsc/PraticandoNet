

---

## Resumo: Versionamento de API em .NET

O versionamento de API � uma pr�tica fundamental para manter a compatibilidade e evolu��o das APIs sem prejudicar os clientes que utilizam vers�es antigas. Neste resumo, exploraremos os principais m�todos de versionamento de API em .NET, incluindo exemplos de como implement�-los e identific�-los nos controladores.

### �ndice

1. [O que � Versionamento de API?](#o-que-�-versionamento-de-api)
2. [Formas de Versionamento](#formas-de-versionamento)
   - [Querystring](#querystring)
   - [URI](#uri)
   - [Headers](#headers)
   - [Media Type](#media-type)
3. [Implementa��o de Versionamento em .NET](#implementa��o-de-versionamento-em-net)
   - [Configurando o Program.cs](#configurando-o-programcs)
   - [Identificando Vers�es nos Controladores](#identificando-vers�es-nos-controladores)
   - [Usando o Atributo `MapToApiVersion`](#usando-o-atributo-maptoapiversion)
   - [Marcando uma Vers�o como Depreciada](#marcando-uma-vers�o-como-depreciada)
4. [Conclus�o](#conclus�o)

---

### 1. O que � Versionamento de API?

O versionamento de API refere-se ao processo de gerenciar e identificar diferentes vers�es de uma API, garantindo que consumidores possam continuar utilizando vers�es anteriores enquanto novas funcionalidades s�o adicionadas ou modificadas. Isso permite a evolu��o da API sem causar quebras para os clientes que ainda dependem de vers�es mais antigas.

---

### 2. Formas de Versionamento

#### 2.1. Querystring

**O que �:**
O versionamento por Querystring utiliza um par�metro na URL para especificar a vers�o da API. Essa abordagem � simples e clara para os consumidores da API.

**Exemplo:**
```http
GET https://api.exemplo.com/produtos?v=1.0
GET https://api.exemplo.com/produtos?v=2.0
```
Aqui, a vers�o da API � especificada pelo par�metro `v` na Querystring.

**Como funciona:**
O cliente informa a vers�o da API na Querystring, e o servidor responde com a vers�o correspondente.

**Vantagens:**
- F�cil de implementar e vis�vel na URL.
- Permite especificar vers�es explicitamente.

**Desvantagens:**
- A URL pode ficar polu�da com muitos par�metros.
- Menos intuitivo para APIs p�blicas.

#### 2.2. URI

**O que �:**
No versionamento por URI, a vers�o da API � parte do caminho da URL, sendo um dos m�todos mais comuns por sua clareza e simplicidade.

**Exemplo:**
```http
GET https://api.exemplo.com/v1/produtos
GET https://api.exemplo.com/v2/produtos
```
A vers�o � especificada diretamente no caminho da URI (`v1` e `v2`).

**Como funciona:**
O cliente faz a requisi��o para uma URI que inclui a vers�o da API. O servidor responde com os recursos correspondentes � vers�o especificada.

**Vantagens:**
- Clareza no caminho da URL.
- F�cil de documentar e entender.

**Desvantagens:**
- Pode resultar em m�ltiplas URIs para a mesma funcionalidade.
- Exige manuten��o de m�ltiplas vers�es ativas.

#### 2.3. Headers

**O que �:**
No versionamento via Headers, a vers�o da API � especificada no cabe�alho HTTP da requisi��o, mantendo a URL limpa.

**Exemplo:**
```http
GET https://api.exemplo.com/produtos
Headers:
    X-Api-Version: 1.0
```
Aqui, o cabe�alho `X-Api-Version` carrega a vers�o da API.

**Como funciona:**
O cliente inclui um cabe�alho na requisi��o que indica a vers�o desejada. O servidor retorna a resposta correspondente.

**Vantagens:**
- Mant�m a URL limpa.
- Pode ser combinado com outras estrat�gias de versionamento.

**Desvantagens:**
- Menos vis�vel e pode ser dif�cil de depurar.
- Requer conhecimento adicional sobre cabe�alhos HTTP.

#### 2.4. Media Type

**O que �:**
O versionamento por Media Type envolve a inclus�o da vers�o da API no tipo de m�dia (Content-Type) enviado ou aceito na requisi��o.

**Exemplo:**
```http
GET https://api.exemplo.com/produtos
Headers:
    Accept: application/vnd.exemplo.v1+json
```
O cabe�alho `Accept` inclui a vers�o da API dentro do tipo de m�dia.

**Como funciona:**
O cliente especifica a vers�o da API que deseja usar no cabe�alho `Accept` ou `Content-Type`. O servidor interpreta o cabe�alho e retorna a resposta adequada.

**Vantagens:**
- Maior flexibilidade e controle sobre o formato dos dados.
- Pode ser combinado com outros m�todos de versionamento.

**Desvantagens:**
- Mais complexo de implementar e entender.
- Pode gerar confus�o entre clientes que n�o conhecem Media Types personalizados.

---

### 3. Implementa��o de Versionamento em .NET

#### 3.1. Configurando o Program.cs

Para implementar o versionamento em uma API .NET, primeiro � necess�rio configurar o `Program.cs` para definir como as vers�es ser�o gerenciadas.

**Exemplo:**
```csharp
var temp = builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
                          new QueryStringApiVersionReader(),
                          new UrlSegmentApiVersionReader());
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```
Aqui, configuramos o versionamento utilizando Querystring e URI, que s�o os m�todos mais utilizados.

#### 3.2. Identificando Vers�es nos Controladores

Para identificar as vers�es nas controladoras, utilizamos o atributo `[ApiVersion]`.

**Exemplo:**
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProdutosController : ControllerBase
{
    // M�todos da API para as diferentes vers�es
}
```
Com essa configura��o, o controlador poder� gerenciar m�ltiplas vers�es da API.

#### 3.3. Usando o Atributo `MapToApiVersion`

O atributo `[MapToApiVersion]` � usado para mapear uma a��o espec�fica a uma vers�o da API.

**Exemplo:**
```csharp
[HttpGet]
[MapToApiVersion("2.0")]
public IActionResult GetV2()
{
    // Implementa��o da vers�o 2.0
    return Ok("Vers�o 2.0");
}
```
Esse atributo � �til quando uma a��o espec�fica est� dispon�vel apenas em uma vers�o.

#### 3.4. Marcando uma Vers�o como Depreciada

Para marcar uma vers�o como depreciada, utilizamos o atributo `[ApiVersion("x.x", Deprecated = true)]`.

**Exemplo:**
```csharp
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProdutosController : ControllerBase
{
    // Implementa��o da vers�o 1.0 que est� depreciada
}
```
Quando uma vers�o � marcada como depreciada, os clientes devem ser informados para migrarem para vers�es mais recentes.

---

### 4. Conclus�o

Cada m�todo de versionamento de API tem suas vantagens e desvantagens, e a escolha do melhor m�todo depende do contexto e das necessidades da aplica��o. Em .NET, o versionamento pode ser configurado de forma flex�vel, permitindo combinar diferentes abordagens para melhor atender �s demandas dos clientes e evoluir a API de forma sustent�vel.

---

Este resumo cobre as principais formas de versionamento de APIs em .NET, com exemplos pr�ticos e explica��es de como implementar cada um deles.