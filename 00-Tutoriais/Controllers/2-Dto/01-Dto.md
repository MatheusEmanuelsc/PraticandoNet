# DTO (Data Transfer Object) em ASP.NET Core

## Índice
1. [Introdução](#introdução)
2. [O que é um DTO?](#o-que-é-um-dto)
3. [Benefícios do uso de DTOs](#benefícios-do-uso-de-dtos)
4. [Criando um DTO](#criando-um-dto)
5. [Configurando o Projeto ASP.NET Core](#configurando-o-projeto-aspnet-core)
6. [Usando DTOs em um Controller](#usando-dtos-em-um-controller)
7. [Conclusão](#conclusão)

## Introdução
Este documento fornece uma visão geral sobre o uso de DTOs (Data Transfer Objects) em ASP.NET Core, além de um tutorial passo a passo para implementar DTOs no seu projeto.

## O que é um DTO?
Um Data Transfer Object (DTO) é um objeto que carrega dados entre processos. Ele é uma forma de encapsular dados e transportar esses dados de forma eficiente entre diferentes camadas de uma aplicação.

## Benefícios do uso de DTOs
- **Segurança**: Expõe apenas os dados necessários, escondendo propriedades sensíveis.
- **Desempenho**: Pode diminuir o tamanho da carga útil ao transportar apenas os dados necessários.
- **Desacoplamento**: Separa a camada de apresentação dos modelos de domínio.
- **Validação**: Facilita a validação de dados de entrada.

## Criando um DTO
Um DTO é normalmente uma classe simples com propriedades que correspondem aos dados que você deseja transportar. Exemplo:

```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

## Configurando o Projeto ASP.NET Core

### 1. Criando um novo projeto ASP.NET Core
- Abra o Visual Studio e selecione **Criar um novo projeto**.
- Escolha **Aplicativo Web ASP.NET Core** e clique em **Avançar**.
- Dê um nome ao seu projeto e clique em **Criar**.
- Selecione **API** e clique em **Criar**.

### 2. Instalando Pacotes Necessários
Para mapear automaticamente entidades para DTOs, podemos usar a biblioteca AutoMapper. Instale o pacote AutoMapper.Extensions.Microsoft.DependencyInjection:

```bash
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### 3. Configurando o AutoMapper
No arquivo `Program.cs`, configure o AutoMapper:

```csharp
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Adicione o AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.Run();
```

Crie um perfil de mapeamento para configurar os mapeamentos entre entidades e DTOs. Adicione uma pasta chamada `Mappings` e crie um arquivo `MappingProfile.cs`:

```csharp
using AutoMapper;
using YourNamespace.Models;
using YourNamespace.Dtos;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
    }
}
```

## Usando DTOs em um Controller
Vamos criar um controlador que usa DTOs. Adicione uma pasta chamada `Controllers` e crie um arquivo `UserController.cs`:

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Dtos;
using YourNamespace.Models;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;

    public UserController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        // Simulação de busca de usuário
        var user = new User { Id = id, Name = "John Doe", Email = "john.doe@example.com" };

        var userDto = _mapper.Map<UserDto>(user);

        return Ok(userDto);
    }
}
```

## Conclusão
DTOs são uma ferramenta poderosa para simplificar e otimizar a transferência de dados em aplicações ASP.NET Core. Eles ajudam a manter a segurança e a performance, além de facilitar a validação de dados. Com este tutorial, você deve estar apto a criar e utilizar DTOs em seus projetos ASP.NET Core.