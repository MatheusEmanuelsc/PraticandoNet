### Documentação com Swagger em .NET 8: Configuração e Uso

Neste tutorial, abordaremos como gerar a documentação para uma Web API utilizando o Swagger em .NET 8. Vamos considerar dois cenários: um em que o Swagger já vem configurado por padrão e outro em que é necessário instalá-lo e configurá-lo manualmente.

### Índice
1. [Introdução ao Swagger](#introdução-ao-swagger)
2. [Cenário 1: Swagger Configurado por Padrão](#cenário-1-swagger-configurado-por-padrão)
   - 2.1 [Verificação da Configuração](#verificação-da-configuração)
   - 2.2 [Testando a Documentação](#testando-a-documentação)
3. [Cenário 2: Instalação e Configuração Manual do Swagger](#cenário-2-instalação-e-configuração-manual-do-swagger)
   - 3.1 [Instalando o Pacote Swagger](#instalando-o-pacote-swagger)
   - 3.2 [Configurando o Swagger no `Program.cs`](#configurando-o-swagger-no-programcs)
   - 3.3 [Personalizando a Documentação](#personalizando-a-documentação)
   - 3.4 [Habilitando Comentários XML](#habilitando-comentários-xml)
4. [Conclusão](#conclusão)

---

### 1. Introdução ao Swagger

Swagger é uma ferramenta popular para a documentação de APIs, oferecendo uma interface interativa onde desenvolvedores podem explorar e testar endpoints. No .NET 8, o Swagger pode ser facilmente integrado e configurado para oferecer uma documentação rica e informativa.

### 2. Cenário 1: Swagger Configurado por Padrão

No .NET 8, ao criar uma nova Web API com o template padrão, o Swagger já vem pré-configurado. Vamos ver como verificar se está funcionando e como utilizá-lo.

#### 2.1 Verificação da Configuração

Após criar uma nova Web API no .NET 8, verifique se o Swagger está habilitado no arquivo `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Aqui, `AddSwaggerGen()` adiciona os serviços necessários para o Swagger, enquanto `UseSwagger()` e `UseSwaggerUI()` habilitam o middleware para gerar e visualizar a documentação.

#### 2.2 Testando a Documentação

Execute a aplicação (`dotnet run` ou `F5` no Visual Studio) e navegue até `http://localhost:<porta>/swagger` no seu navegador. Você deverá ver a interface do Swagger, onde pode explorar os endpoints disponíveis.

### 3. Cenário 2: Instalação e Configuração Manual do Swagger

Neste cenário, vamos considerar que você tem uma Web API onde o Swagger não foi configurado por padrão. Vamos ver como instalá-lo e configurá-lo manualmente.

#### 3.1 Instalando o Pacote Swagger

Primeiro, instale o pacote `Swashbuckle.AspNetCore` via NuGet. Você pode fazer isso utilizando o comando `dotnet` ou através do gerenciador de pacotes no Visual Studio:

```bash
dotnet add package Swashbuckle.AspNetCore
```

#### 3.2 Configurando o Swagger no `Program.cs`

Após instalar o pacote, abra o arquivo `Program.cs` e adicione as configurações do Swagger:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Minha API v1");
        options.RoutePrefix = string.Empty; // Para acessar o Swagger na raiz
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Aqui, configuramos o Swagger UI com o endpoint padrão (`/swagger/v1/swagger.json`) e definimos `RoutePrefix` como uma string vazia para que a interface do Swagger esteja disponível diretamente na raiz (`http://localhost:<porta>`).

#### 3.3 Personalizando a Documentação

Você pode personalizar a documentação, adicionando informações como título, descrição e versão da API:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Minha API",
        Version = "v1",
        Description = "Exemplo de uma API documentada com Swagger no .NET 8",
        Contact = new OpenApiContact
        {
            Name = "Seu Nome",
            Email = "seu.email@dominio.com"
        }
    });
});
```

#### 3.4 Habilitando Comentários XML

Para enriquecer ainda mais a documentação, você pode habilitar comentários XML no Swagger. Primeiro, gere os comentários no seu projeto:

1. No Visual Studio, clique com o botão direito no projeto e vá em `Propriedades`.
2. Na aba `Compilar`, marque a opção `Arquivo de documentação XML` e defina o caminho desejado (por exemplo, `bin\Debug\net8.0\MeuProjeto.xml`).

Depois, configure o Swagger para incluir esses comentários:

```csharp
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
c.IncludeXmlComments(xmlPath);
```

Essa configuração permitirá que o Swagger exiba os comentários dos seus métodos e classes, tornando a documentação mais útil e informativa.

### 4. Conclusão

Neste tutorial, abordamos como configurar e utilizar o Swagger em uma Web API .NET 8, considerando tanto o cenário onde ele já vem pré-configurado quanto o cenário onde a configuração é feita manualmente. O Swagger é uma ferramenta poderosa que, quando bem configurada, pode facilitar muito o trabalho de desenvolvedores e consumidores da sua API.

Essa documentação será uma excelente referência para o desenvolvimento e uso da sua Web API, permitindo que outros desenvolvedores entendam e interajam com seus endpoints de forma clara e eficaz.